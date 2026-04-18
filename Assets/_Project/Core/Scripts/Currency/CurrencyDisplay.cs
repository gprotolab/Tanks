using ANut.Core.Utils;
using TMPro;
using R3;
using UnityEngine;
using VContainer;

namespace ANut.Core.Currency
{
    public class CurrencyDisplay : MonoBehaviour
    {
        [SerializeField] private CurrencyType _currencyType = CurrencyType.Coins;
        [SerializeField] private TMP_Text _amountText;

        private ICurrencyService _currencyService;

        [Inject]
        public void Construct(ICurrencyService currencyService)
        {
            _currencyService = currencyService;

            _currencyService.GetBalance(_currencyType)
                .Subscribe(balance => _amountText.text = CostFormatter.Detailed(balance))
                .AddTo(this);
        }
    }
}