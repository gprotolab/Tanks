using System;
using ANut.Core.Save;
using ANut.Core;
using Game.Equipment;
using Newtonsoft.Json;
using R3;

namespace Game.Battle
{
    public class BattleStatsDataService : SaveModuleBase<BattleStatsDataService.Save>, IDisposable
    {
        public sealed class Save
        {
            [JsonProperty("total_battles")] public int TotalBattles { get; set; }
            [JsonProperty("total_wins")] public int TotalWins { get; set; }
            [JsonProperty("battles_on_equip")] public int BattlesOnCurrentEquip { get; set; }
            [JsonProperty("poor_finish_streak")] public int PoorFinishStreak { get; set; }
        }

        private readonly EquipmentDataService _equipment;
        private readonly CompositeDisposable _disposables = new();

        public BattleStatsDataService(EquipmentDataService equipment)
        {
            _equipment = equipment;

            _equipment.MaxLevelChanged
                .Subscribe(_ => ResetDifficultyProgress())
                .AddTo(_disposables);
        }

        public override string Key => "battle_stats";

        public int TotalBattles => Data.TotalBattles;
        public int TotalWins => Data.TotalWins;
        public int BattlesOnCurrentEquip => Data.BattlesOnCurrentEquip;
        public int PoorFinishStreak => Data.PoorFinishStreak;

        public void RecordBattleResult(int place, bool isWin, int poorFinishThreshold)
        {
            Data.TotalBattles++;
            if (isWin) Data.TotalWins++;

            Data.BattlesOnCurrentEquip++;

            if (place > poorFinishThreshold)
                Data.PoorFinishStreak++;
            else
                Data.PoorFinishStreak = 0;

            MarkDirty();
        }

        public void Dispose() => _disposables.Dispose();

        private void ResetDifficultyProgress()
        {
            Data.BattlesOnCurrentEquip = 0;
            Data.PoorFinishStreak = 0;
            MarkDirty();
            Log.Info("[DDA] New max level equipped — difficulty progress reset.");
        }
    }
}