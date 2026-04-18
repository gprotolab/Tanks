namespace Game.Equipment
{
    public class TankPartData
    {
        public TankPartType Type { get; }
        public int Level { get; }

        public TankPartData(TankPartType type, int level)
        {
            Type = type;
            Level = level;
        }
    }

    public enum TankPartType
    {
        Turret,
        Chassis
    }
}