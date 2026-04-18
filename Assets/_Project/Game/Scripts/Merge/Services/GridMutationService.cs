using ANut.Core;
using Game.Equipment;

namespace Game.Merge
{
    public sealed class GridMutationService
    {
        private readonly MergeModel _model;
        private readonly MergeRuleService _rules;
        private readonly MergeSettings _settings;
        private readonly MergeDataService _data;

        public GridMutationService(
            MergeModel model,
            MergeRuleService rules,
            MergeSettings settings,
            MergeDataService data)
        {
            _model = model;
            _rules = rules;
            _settings = settings;
            _data = data;
        }

        public bool TryMerge(int fromCol, int fromRow, int toCol, int toRow)
        {
            var from = _model.GetCell(fromCol, fromRow);
            var to = _model.GetCell(toCol, toRow);
            if (from.IsEmpty || to.IsEmpty) return false;

            var result = _rules.TryMerge(from.Part, to.Part, _settings.MaxPartLevel);
            if (!result.Success) return false;

            _model.ClearCell(fromCol, fromRow);
            _model.SetCell(toCol, toRow, result.Part);
            _data.TrackMaxLevel(result.Part.Type, result.Part.Level);
            return true;
        }

        public void PlacePart(int col, int row, TankPartData part)
            => _model.SetCell(col, row, part);

        public void MovePart(int fromCol, int fromRow, int toCol, int toRow)
        {
            var cell = _model.GetCell(fromCol, fromRow);
            if (cell.IsEmpty)
            {
                Log.Warning("[GridMutationService] MovePart: source ({0},{1}) is empty", fromCol, fromRow);
                return;
            }

            _model.ClearCell(fromCol, fromRow);
            _model.SetCell(toCol, toRow, cell.Part);
        }

        public void RemovePart(int col, int row) => _model.ClearCell(col, row);
        public void UnlockNextCell() => _model.UnlockNextCell();
    }
}