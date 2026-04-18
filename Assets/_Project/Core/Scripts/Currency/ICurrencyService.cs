using System.Collections.Generic;
using R3;

namespace ANut.Core.Currency
{
    public interface ICurrencyService
    {
        ReadOnlyReactiveProperty<long> GetBalance(CurrencyType type);

        bool CanAfford(CurrencyType type, long amount);
        bool TrySpend(CurrencyType type, long amount, string reason);
        void Add(CurrencyType type, long amount, string reason);
    }
}