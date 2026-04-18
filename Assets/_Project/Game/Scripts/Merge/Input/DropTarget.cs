namespace Game.Merge
{
    public readonly struct DropTarget
    {
        public readonly DropZoneType Zone;
        public readonly int Col;
        public readonly int Row;

        private DropTarget(DropZoneType zone, int col, int row)
        {
            Zone = zone;
            Col = col;
            Row = row;
        }

        public static DropTarget GridCell(int col, int row)
        {
            return new DropTarget(DropZoneType.GridCell, col, row);
        }

        public static DropTarget Tank()
        {
            return new DropTarget(DropZoneType.TankSlot, -1, -1);
        }

        public static DropTarget Sell()
        {
            return new DropTarget(DropZoneType.SellZone, -1, -1);
        }

        public static DropTarget None()
        {
            return new DropTarget(DropZoneType.None, -1, -1);
        }
    }
}