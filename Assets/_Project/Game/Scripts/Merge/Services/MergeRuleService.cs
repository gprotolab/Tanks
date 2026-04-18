using Game.Equipment;

namespace Game.Merge
{
    public sealed class MergeRuleService
    {
        public bool CanMerge(TankPartData a, TankPartData b)
            => a != null && b != null && a.Type == b.Type && a.Level == b.Level;

        public MergeResult TryMerge(TankPartData a, TankPartData b, int maxLevel)
        {
            if (!CanMerge(a, b)) return MergeResult.Fail(a);
            int next = a.Level + 1;
            return next > maxLevel
                ? MergeResult.Fail(a)
                : MergeResult.Ok(new TankPartData(a.Type, next));
        }
    }
}