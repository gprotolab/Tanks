using System;
using ANut.Core.UI;
using R3;
using UnityEngine;

namespace Game.Idle
{
    public class IdleView : MonoBehaviour
    {
        [SerializeField] private IdleClickAreaView _clickArea;
        [SerializeField] private LabeledButtonView _upgradeButton;
        [SerializeField] private CoinAnimationSpawner _coinSpawner;

        public Observable<Unit> OnAreaClicked => _clickArea.OnClicked;

        public Observable<Unit> OnUpgradeClicked => _upgradeButton.OnClicked;

        public void SetUpgradeCost(string formattedCost) => _upgradeButton.SetLabel(formattedCost);

        public void SetUpgradeInteractable(bool value) => _upgradeButton.SetInteractable(value);

        public void SpawnCoinAnimation(long amount) => _coinSpawner.SpawnCoin(amount);
    }
}