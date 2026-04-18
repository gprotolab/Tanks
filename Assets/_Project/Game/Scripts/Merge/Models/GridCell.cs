using Game.Equipment;

namespace Game.Merge
{
    public readonly struct GridCell
    {
        public readonly bool IsEmpty;
        public readonly TankPartData Part;

        private GridCell(TankPartData part)
        {
            IsEmpty = part == null;
            Part = part;
        }

        public static GridCell Empty() => new(null);
        public static GridCell WithPart(TankPartData part) => new(part);
    }
}