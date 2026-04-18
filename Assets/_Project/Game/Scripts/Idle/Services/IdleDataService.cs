using ANut.Core.Save;
using Newtonsoft.Json;

namespace Game.Idle
{
    public class IdleDataService : SaveModuleBase<IdleDataService.Save>
    {
        public sealed class Save
        {
            [JsonProperty("income_level")] public int IncomeLevel { get; set; } = 1;
        }

        public override string Key => "idle";

        public int IncomeLevel => Data.IncomeLevel;

        public void IncrementIncomeLevel()
        {
            Data.IncomeLevel++;
            MarkDirty();
        }
    }
}