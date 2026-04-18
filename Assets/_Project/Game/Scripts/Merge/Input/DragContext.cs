using Game.Equipment;

namespace Game.Merge
{
    public class DragContext
    {
        public int SourceCol { get; }
        public int SourceRow { get; }
        public TankPartData Part { get; }
        public MergePartView PartView { get; }

        public DragContext(int sourceCol, int sourceRow, TankPartData part, MergePartView partView)
        {
            SourceCol = sourceCol;
            SourceRow = sourceRow;
            Part = part;
            PartView = partView;
        }
    }
}