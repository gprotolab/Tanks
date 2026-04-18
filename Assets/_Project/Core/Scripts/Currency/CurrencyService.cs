using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using R3;
using ANut.Core.Save;

namespace ANut.Core.Currency
{
    public class CurrencyService : SaveModuleBase<CurrencyService.Save>, ICurrencyService, IDisposable
    {
        // Nested Save — only what goes to disk 
        public sealed class Save
        {
            [JsonProperty("balances")] public Dictionary<CurrencyType, long> Balances { get; set; } = new();
        }

        // ISaveModule
        public override string Key => "currency";

        protected override void OnAfterDeserialize()
        {
            // Sync reactive properties with loaded data
            foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
            {
                long value = Data.Balances.GetValueOrDefault(type, 0L);
                if (_balanceProperties.TryGetValue(type, out var rp))
                    rp.Value = value;
            }
        }

        // Runtime — reactive properties
        private readonly Dictionary<CurrencyType, ReactiveProperty<long>> _balanceProperties = new();

        public CurrencyService()
        {
            foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
                _balanceProperties[type] = new ReactiveProperty<long>(0L);
        }

        // ICurrencyService 

        public ReadOnlyReactiveProperty<long> GetBalance(CurrencyType type)
            => _balanceProperties[type];

        public bool CanAfford(CurrencyType type, long amount)
            => _balanceProperties[type].Value >= amount;

        public void Add(CurrencyType type, long amount, string reason)
        {
            if (amount <= 0)
            {
                Log.Warning("[CurrencyService] Trying to add non-positive amount {0} {1}, ignored", amount, type);
                return;
            }

            _balanceProperties[type].Value += amount;
            Data.Balances[type] = _balanceProperties[type].Value;
            MarkDirty();
        }

        public bool TrySpend(CurrencyType type, long amount, string reason)
        {
            if (amount <= 0)
            {
                Log.Warning("[CurrencyService] Trying to spend non-positive amount {0} {1}, ignored", amount, type);
                return false;
            }

            if (!CanAfford(type, amount))
            {
                Log.Warning("[CurrencyService] Not enough {0}: need {1}, have {2}", type, amount,
                    _balanceProperties[type].Value);
                return false;
            }

            _balanceProperties[type].Value -= amount;
            Data.Balances[type] = _balanceProperties[type].Value;
            MarkDirty();
            return true;
        }

        // IDisposable

        public void Dispose()
        {
            foreach (var rp in _balanceProperties.Values)
                rp.Dispose();
        }
    }
}