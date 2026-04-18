using TMPro;
using UnityEngine;

namespace Game.Merge
{
    public class MergeCellView : MonoBehaviour
    {
        private enum VisualState
        {
            None,
            Unlocked,
            Selected,
            MergeAvailable,
            PurchaseAvailable
        }

        [SerializeField] private GameObject _unlockedState;
        [SerializeField] private GameObject _selectedState;
        [SerializeField] private GameObject _mergeAvailableState;
        [SerializeField] private Transform _slotPosition;
        [SerializeField] private GameObject _purchaseAvailableState;
        [SerializeField] private TextMeshPro _costLabel;

        private HighlightType _highlightType = HighlightType.None;
        private bool _hasMergeHint;
        private bool _canPurchaseCell;

        public int Col { get; private set; }
        public int Row { get; private set; }
        public CellMode Mode { get; private set; }

        public Vector3 SlotWorldPosition => _slotPosition.position;

        public void Initialize(int col, int row)
        {
            Col = col;
            Row = row;
            _highlightType = HighlightType.None;
            _hasMergeHint = false;
            _canPurchaseCell = false;
            SetCellMode(CellMode.Active);
        }

        public void SetCellMode(CellMode mode, string cost = null, bool canPurchase = true)
        {
            Mode = mode;
            _canPurchaseCell = canPurchase;
            gameObject.SetActive(mode != CellMode.Hidden);

            if (_costLabel != null && cost != null)
                _costLabel.text = cost;

            RefreshVisualState();
        }

        public void SetSelectedHighlight(HighlightType type)
        {
            if (Mode != CellMode.Active)
                return;

            _highlightType = type;
            RefreshVisualState();
        }

        public void SetMergeHint(bool active)
        {
            if (Mode != CellMode.Active)
                return;

            _hasMergeHint = active;
            RefreshVisualState();
        }

        private void RefreshVisualState()
        {
            if (Mode == CellMode.Hidden)
            {
                ApplyVisualState(VisualState.None);
                return;
            }

            if (Mode == CellMode.Purchasable)
            {
                ApplyVisualState(VisualState.PurchaseAvailable);
                return;
            }

            ApplyVisualState(ResolveActiveCellVisualState());
        }

        private VisualState ResolveActiveCellVisualState()
        {
            return _highlightType switch
            {
                HighlightType.CanMerge => VisualState.MergeAvailable,
                HighlightType.CanPlace => VisualState.Selected,
                HighlightType.Invalid => VisualState.Selected,
                HighlightType.None when _hasMergeHint => VisualState.MergeAvailable,
                _ => VisualState.Unlocked
            };
        }

        private void ApplyVisualState(VisualState state)
        {
            SetStateActive(_unlockedState, state == VisualState.Unlocked);
            SetStateActive(_selectedState, state == VisualState.Selected);
            SetStateActive(_purchaseAvailableState, state == VisualState.PurchaseAvailable);
            SetStateActive(_mergeAvailableState, state == VisualState.MergeAvailable);
        }

        private static void SetStateActive(GameObject state, bool isActive)
        {
            if (state == null)
                return;

            state.SetActive(isActive);
        }
    }
}