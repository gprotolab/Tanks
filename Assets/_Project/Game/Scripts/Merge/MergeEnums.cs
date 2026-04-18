namespace Game.Merge
{
    public enum CellMode
    {
        Active,
        Purchasable,
        Hidden
    }

    public enum EquipResult
    {
        Equipped,
        Merged,
        Swapped,
        InvalidType,
        InvalidLevel
    }

    public enum HighlightType
    {
        None,
        CanPlace,
        CanMerge,
        Invalid
    }
}