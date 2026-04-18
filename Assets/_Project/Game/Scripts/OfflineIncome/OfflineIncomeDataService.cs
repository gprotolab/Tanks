using System;
using ANut.Core.Save;
using Newtonsoft.Json;

namespace Game.Offline
{
    public class OfflineIncomeDataService : SaveModuleBase<OfflineIncomeDataService.Save>
    {
        public sealed class Save
        {
            [JsonProperty("last_session_end")] public long LastSessionEndUnix { get; set; }
            [JsonProperty("pending_reward")] public long PendingReward { get; set; }
        }

        public override string Key => "offline_income";

        public long LastSessionEndUnix => Data.LastSessionEndUnix;
        public long PendingReward => Data.PendingReward;
        public bool HasPendingReward => Data.PendingReward > 0;

        public void RecordSessionEnd()
        {
            Data.LastSessionEndUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            MarkDirty();
        }

        public void SetPendingReward(long amount)
        {
            Data.PendingReward = amount;
            MarkDirty();
        }

        public void ClearPendingReward()
        {
            Data.PendingReward = 0;
            MarkDirty();
        }
    }
}