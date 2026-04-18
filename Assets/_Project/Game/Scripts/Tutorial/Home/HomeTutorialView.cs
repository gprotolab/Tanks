using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tutorial
{
    public class HomeTutorialView : MonoBehaviour
    {
        public event Action BattleClicked;


        [Serializable]
        public class TutorialStepData
        {
            [SerializeField] public GameObject[] Activate;
            [SerializeField] public GameObject[] Deactivate;
        }

        [Header("Battle Button")] [SerializeField]
        private Button _battleButton;

        [Header("Step: Init")] [SerializeField]
        private TutorialStepData _stepInit;

        [Header("Step: TapTank")] [SerializeField]
        private TutorialStepData _stepTapTank;

        [Header("Step: BuyFirstPart")] [SerializeField]
        private TutorialStepData _stepBuyFirstPart;

        [Header("Step: TapTankAgain")] [SerializeField]
        private TutorialStepData _stepTapTankAgain;

        [Header("Step: BuySecondPart")] [SerializeField]
        private TutorialStepData _stepBuySecondPart;

        [Header("Step: MergeParts")] [SerializeField]
        private TutorialStepData _stepMergeParts;

        [Header("Step: EquipPart")] [SerializeField]
        private TutorialStepData _stepEquipPart;

        [Header("Step: ShowBattleButton")] [SerializeField]
        private TutorialStepData _stepShowBattleButton;

        private void Awake()
        {
            _battleButton.onClick.AddListener(OnBattleButtonClicked);
        }

        private void OnDestroy()
        {
            _battleButton.onClick.RemoveListener(OnBattleButtonClicked);
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);

        public void ApplyStepInit() => Apply(_stepInit);
        public void ApplyStepTapTank() => Apply(_stepTapTank);
        public void ApplyStepBuyFirstPart() => Apply(_stepBuyFirstPart);
        public void ApplyStepTapTankAgain() => Apply(_stepTapTankAgain);
        public void ApplyStepBuySecondPart() => Apply(_stepBuySecondPart);
        public void ApplyStepMergeParts() => Apply(_stepMergeParts);
        public void ApplyStepEquipPart() => Apply(_stepEquipPart);
        public void ApplyStepShowBattleButton() => Apply(_stepShowBattleButton);


        private static void Apply(TutorialStepData step)
        {
            if (step == null)
                return;

            if (step.Activate != null)
                foreach (var obj in step.Activate)
                    if (obj != null)
                        obj.SetActive(true);

            if (step.Deactivate != null)
                foreach (var obj in step.Deactivate)
                    if (obj != null)
                        obj.SetActive(false);
        }

        private void OnBattleButtonClicked()
        {
            BattleClicked?.Invoke();
        }
    }
}