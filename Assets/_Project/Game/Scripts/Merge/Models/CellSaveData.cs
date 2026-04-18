using Game.Equipment;

namespace Game.Merge
{
    [System.Serializable]
    public class CellSaveData
    {
        public int Col = 0;
        public int Row = 0;
        public int Type = 0;
        public int Level = 0;

        public static CellSaveData From(int col, int row, TankPartData part) => new()
        {
            Col = col,
            Row = row,
            Type = (int) part.Type,
            Level = part.Level
        };

        public TankPartData ToPartData() => new((TankPartType) Type, Level);
    }
}