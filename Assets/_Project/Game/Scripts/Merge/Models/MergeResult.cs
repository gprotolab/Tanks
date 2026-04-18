using Game.Equipment;

namespace Game.Merge
{
    public readonly struct MergeResult
    {
        public readonly bool Success;
        public readonly TankPartData Part;

        public static MergeResult Ok(TankPartData part) => new(true, part);
        public static MergeResult Fail(TankPartData original) => new(false, original);

        private MergeResult(bool success, TankPartData part)
        {
            Success = success;
            Part = part;
        }
    }
}