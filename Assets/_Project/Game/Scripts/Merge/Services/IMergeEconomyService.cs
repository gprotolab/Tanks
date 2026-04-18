using Game.Equipment;

namespace Game.Merge
{
    public interface IMergeEconomyService
    {
        bool CanBuyPart();
        long GetBuyPrice();
        TankPartData TryBuyPart();
        long GetSellPrice(TankPartData part);
        long GetSellPriceAtCell(int col, int row);
        void ExecuteSell(int col, int row);
        bool CanExpandGrid();
        long GetExpandPrice();
        bool TryExpandGrid();
    }
}