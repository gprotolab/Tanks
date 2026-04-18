using System;
using ANut.Core.UI;
using R3;
using UnityEngine;

namespace Game.Merge
{
    public class MergePurchaseView : MonoBehaviour
    {
        [SerializeField] private LabeledButtonView _button;

        public Observable<Unit> OnBuyClicked => _button.OnClicked;

        public void SetCost(string formattedCost) => _button.SetLabel(formattedCost);

        public void SetInteractable(bool value) => _button.SetInteractable(value);

        public void PlayShakeAnimation() => _button.PlayShakeAnimation();
    }
}