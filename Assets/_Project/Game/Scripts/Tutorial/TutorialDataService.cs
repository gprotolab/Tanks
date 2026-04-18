using System.Collections.Generic;
using ANut.Core.Save;
using Newtonsoft.Json;

namespace Game.Tutorial
{
    public static class TutorialStepKeys
    {
        public const string HomeTapTank = "home_tap_tank";
        public const string HomeBuyFirstPart = "home_buy_first_part";
        public const string HomeTapTankAgain = "home_tap_tank_again";
        public const string HomeBuySecondPart = "home_buy_second_part";
        public const string HomeMergeParts = "home_merge_parts";
        public const string HomeEquipPart = "home_equip_part";
    }

    public class TutorialDataService : SaveModuleBase<TutorialDataService.Save>
    {
        public sealed class Save
        {
            [JsonProperty("home_done")] public bool HomeDone { get; set; }
            [JsonProperty("battle_done")] public bool BattleDone { get; set; }
            [JsonProperty("completed_steps")] public HashSet<string> CompletedSteps { get; set; } = new();
        }

        public override string Key => "tutorial";

        public bool IsTutorialDone => Data.HomeDone && Data.BattleDone;
        public bool IsHomeDone => Data.HomeDone;
        public bool IsBattleDone => Data.BattleDone;

        protected override void OnAfterDeserialize()
        {
            Data.CompletedSteps ??= new HashSet<string>();
        }

        public bool IsStepDone(string key)
        {
            return Data.HomeDone || Data.CompletedSteps.Contains(key);
        }

        public void CompleteStep(string key)
        {
            if (Data.HomeDone || Data.CompletedSteps.Contains(key))
                return;

            Data.CompletedSteps.Add(key);
            MarkDirty();
        }

        public void CompleteHome()
        {
            if (Data.HomeDone)
                return;

            Data.HomeDone = true;
            Data.CompletedSteps.Clear();
            MarkDirty();
        }

        public void CompleteBattle()
        {
            if (Data.BattleDone)
                return;

            Data.BattleDone = true;
            MarkDirty();
        }
    }
}