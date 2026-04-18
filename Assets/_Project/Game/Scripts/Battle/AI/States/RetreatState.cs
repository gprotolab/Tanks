using Game.Core.FSM;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Battle
{
    public class RetreatState : IState
    {
        private const float ThreatCheckInterval = 0.2f;
        private const int CandidateCount = 8;

        private readonly BotBrain _bot;

        private Vector3 _threatPosition;
        private bool _hasDestination;
        private Tank _cachedThreat;
        private float _threatCheckTimer;

        public bool IsSafe { get; private set; }

        public RetreatState(BotBrain bot) => _bot = bot;

        public void OnEnter()
        {
            IsSafe = false;
            _hasDestination = false;
            _cachedThreat = null;
            _threatCheckTimer = 0f;

            var closestThreat = _bot.Registry.FindClosestEnemy(
                _bot.transform.position, 50f, _bot.ControlledTank);

            if (closestThreat != null)
            {
                _threatPosition = closestThreat.transform.position;
                CalculateRetreatDestination();
            }
            else
            {
                IsSafe = true;
            }
        }

        public void OnTick(float dt)
        {
            _threatCheckTimer -= dt;
            if (_threatCheckTimer <= 0f)
            {
                _threatCheckTimer = ThreatCheckInterval;
                _cachedThreat = _bot.Registry.FindClosestEnemy(
                    _bot.transform.position, 50f, _bot.ControlledTank);
            }

            if (_cachedThreat == null)
            {
                IsSafe = true;
                return;
            }

            float distToThreat = Vector3.Distance(_bot.transform.position, _cachedThreat.transform.position);
            if (distToThreat >= _bot.Profile.SafeDistance)
            {
                IsSafe = true;
                return;
            }

            if (_hasDestination && _bot.Agent.isOnNavMesh &&
                (!_bot.Agent.hasPath || _bot.Agent.remainingDistance < _bot.Profile.DestinationReachedThreshold))
            {
                _threatPosition = _cachedThreat.transform.position;
                CalculateRetreatDestination();
            }

            if (_bot.Agent.isOnNavMesh && _bot.Agent.hasPath)
            {
                var direction = _bot.Agent.steeringTarget - _bot.transform.position;
                direction.y = 0f;

                if (direction.sqrMagnitude > 0.01f)
                    _bot.DesiredMoveDirection = direction.normalized;
            }
            else
            {
                var awayDir = _bot.transform.position - _cachedThreat.transform.position;
                awayDir.y = 0f;

                if (awayDir.sqrMagnitude > 0.01f)
                    _bot.DesiredMoveDirection = awayDir.normalized;
            }
        }

        public void OnExit()
        {
            _cachedThreat = null;
            _threatCheckTimer = 0f;
            _bot.ResetMovement();
        }

        private void CalculateRetreatDestination()
        {
            if (!_bot.Agent.isOnNavMesh) return;

            Vector3 bestPos = Vector3.zero;
            float bestDist = -1f;

            for (int i = 0; i < CandidateCount; i++)
            {
                float angle = i * (360f / CandidateCount);
                var dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                var candidate = _bot.transform.position + dir * _bot.Profile.RetreatDistance;

                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, _bot.Profile.NavMeshSampleRadius,
                        NavMesh.AllAreas))
                    continue;

                float dist = Vector3.Distance(hit.position, _threatPosition);
                if (dist > bestDist)
                {
                    bestDist = dist;
                    bestPos = hit.position;
                }
            }

            if (bestDist > 0f)
            {
                if (_bot.Profile.SeeksWallCover)
                    bestPos = TryApplyWallCoverBias(bestPos, bestDist);

                _bot.Agent.SetDestination(bestPos);
                _hasDestination = true;
            }
            else
            {
                IsSafe = true;
            }
        }

        private Vector3 TryApplyWallCoverBias(Vector3 bestPos, float bestDist)
        {
            var coverDir = bestPos - _bot.transform.position;
            coverDir.y = 0f;

            if (coverDir.sqrMagnitude <= 0.01f) return bestPos;

            var biasedDir = AddWallCoverBias(coverDir.normalized);
            var biasedCandidate = _bot.transform.position + biasedDir * _bot.Profile.RetreatDistance;

            if (!NavMesh.SamplePosition(biasedCandidate, out NavMeshHit hit, _bot.Profile.NavMeshSampleRadius,
                    NavMesh.AllAreas))
                return bestPos;

            return Vector3.Distance(hit.position, _threatPosition) >= bestDist * 0.8f
                ? hit.position
                : bestPos;
        }

        private Vector3 AddWallCoverBias(Vector3 baseDirection)
        {
            var pos = _bot.transform.position;
            var right = Vector3.Cross(Vector3.up, baseDirection).normalized;

            bool hitLeft = Physics.Raycast(pos, -right, out RaycastHit leftHit, _bot.Profile.RetreatDistance);
            bool hitRight = Physics.Raycast(pos, right, out RaycastHit rightHit, _bot.Profile.RetreatDistance);

            if (hitLeft && (!hitRight || leftHit.distance < rightHit.distance))
                return (baseDirection - right * 0.5f).normalized;
            if (hitRight && (!hitLeft || rightHit.distance < leftHit.distance))
                return (baseDirection + right * 0.5f).normalized;

            return baseDirection;
        }
    }
}