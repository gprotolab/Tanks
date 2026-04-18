using Game.Core.FSM;
using UnityEngine;
using UnityEngine.AI;
using R3;

namespace Game.Battle
{
    public class BotBrain : MonoBehaviour, ITankInput
    {
        [SerializeField] private Tank _tank;
        [SerializeField] private TankAutoAim _autoAim;
        [SerializeField] private NavMeshAgent _agent;

        public Tank ControlledTank => _tank;
        public TankAutoAim AutoAim => _autoAim;
        public TankRegistry Registry { get; private set; }
        public NavMeshAgent Agent => _agent;
        public BotBehaviorConfigSO.BotBehaviorProfile Profile { get; private set; }

        public Vector3 DesiredMoveDirection { get; set; }

        Vector3 ITankInput.MoveDirection => DesiredMoveDirection;
        bool ITankInput.IsMoving => DesiredMoveDirection.sqrMagnitude > 0.01f;

        private TransitionStateMachine _fsm;
        private IState _patrolState;
        private bool _isPaused;

        public void Init(BotBehaviorConfigSO.BotBehaviorProfile profile, TankRegistry registry)
        {
            Profile = profile;
            Registry = registry;

            Agent.updatePosition = false;
            Agent.updateRotation = false;

            if (profile == null) return;

            _fsm = BuildFsm();

            _tank.Respawned
                .Subscribe(_ => _fsm.ForceState(_patrolState))
                .AddTo(this);

            _tank.FrozenChanged
                .Subscribe(SetFrozen)
                .AddTo(this);
        }

        public void ResetMovement()
        {
            DesiredMoveDirection = Vector3.zero;

            if (Agent != null && Agent.isOnNavMesh)
                Agent.ResetPath();
        }

        private TransitionStateMachine BuildFsm()
        {
            var patrol = new PatrolState(this);
            var chase = new ChaseState(this);
            var attack = new AttackState(this);
            var retreat = new RetreatState(this);

            _patrolState = patrol;

            var fsm = new TransitionStateMachine();

            fsm.AddTransition(patrol, chase, () => patrol.HasSpottedTarget);

            fsm.AddTransition(chase, patrol, () => chase.IsTargetLost);
            fsm.AddTransition(chase, attack, () => chase.IsTargetInRange);

            fsm.AddTransition(attack, patrol, () => attack.IsTargetLost);
            fsm.AddTransition(attack, chase, () => attack.IsAutoAimLost);
            fsm.AddTransition(attack, retreat, () => attack.ShouldRetreat);

            fsm.AddTransition(retreat, patrol, () => retreat.IsSafe);

            fsm.Init(patrol);
            return fsm;
        }

        private void SetFrozen(bool frozen)
        {
            _isPaused = frozen;

            if (Agent != null && Agent.isOnNavMesh)
            {
                Agent.isStopped = frozen;
                if (frozen) Agent.velocity = Vector3.zero;
            }

            if (frozen) DesiredMoveDirection = Vector3.zero;
        }

        private void Update()
        {
            if (_isPaused || _fsm == null) return;

            if (Agent != null && Agent.isOnNavMesh)
                Agent.nextPosition = transform.position;

            _fsm.Tick(Time.deltaTime);
        }
    }
}