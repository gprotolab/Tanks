using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Merge
{
    public class MergeDragInput : MonoBehaviour
    {
        private const float RaycastMaxDistance = 100f;

        [Header("Raycast")] [SerializeField] private Camera _gridCamera;
        [SerializeField] private LayerMask _raycastLayerMask;

        [Header("Settings")] [SerializeField] private float _dragThreshold = 5f;

        private MergeGridView _gridView;
        private DragStateMachine _stateMachine = new();

        private readonly ReactiveProperty<bool> _isDragging = new(false);
        private readonly Subject<(int col, int row)> _dragStarted = new();
        private readonly Subject<(Vector2 pos, DropTarget t)> _dragMoved = new();
        private readonly Subject<DropTarget> _dragDropped = new();
        private readonly Subject<Unit> _dragCancelled = new();
        private readonly Subject<(int col, int row)> _cellTapped = new();

        public ReadOnlyReactiveProperty<bool> IsDragging => _isDragging;
        public Observable<(int col, int row)> DragStarted => _dragStarted;
        public Observable<(Vector2 pos, DropTarget target)> DragMoved => _dragMoved;
        public Observable<DropTarget> DragDropped => _dragDropped;
        public Observable<Unit> DragCancelled => _dragCancelled;
        public Observable<(int col, int row)> CellTapped => _cellTapped;

        public void Initialize(MergeGridView gridView) => _gridView = gridView;

        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null) return;

            bool pressed = pointer.press.isPressed;
            Vector2 screenPos = pointer.position.ReadValue();

            switch (_stateMachine.Phase)
            {
                case DragPhase.Idle:
                    if (pointer.press.wasPressedThisFrame)
                        HandlePress(screenPos);
                    break;

                case DragPhase.Pressing:
                    if (!pressed)
                        HandleRelease();
                    else if (_stateMachine.TryEscalateToDrag(screenPos, _dragThreshold))
                        HandleDragStart(screenPos);
                    break;

                case DragPhase.Dragging:
                    if (!pressed) HandleDrop(screenPos);
                    else HandleDragMove(screenPos);
                    break;
            }
        }

        private void HandlePress(Vector2 screenPos)
        {
            if (!TryGetGridCellFromScreenPos(screenPos, out int col, out int row)) return;
            bool hasPart = _gridView != null && _gridView.HasPart(col, row);
            _stateMachine.TryStartPress(new Vector2Int(col, row), screenPos, hasPart);
        }

        private void HandleRelease()
        {
            if (!_stateMachine.HasPartAtSource)
                _cellTapped.OnNext((_stateMachine.SourceCell.x, _stateMachine.SourceCell.y));

            _stateMachine.Release();
            _isDragging.Value = false;
        }

        private void HandleDragStart(Vector2 screenPos)
        {
            if (!_stateMachine.HasPartAtSource)
            {
                _stateMachine.Release();
                return;
            }

            _isDragging.Value = true;
            _dragStarted.OnNext((_stateMachine.SourceCell.x, _stateMachine.SourceCell.y));
            HandleDragMove(screenPos);
        }

        private void HandleDragMove(Vector2 screenPos)
        {
            var target = ResolveDropTarget(screenPos);
            _dragMoved.OnNext((screenPos, target));
        }

        private void HandleDrop(Vector2 screenPos)
        {
            var target = ResolveDropTarget(screenPos);
            _stateMachine.Release();
            _isDragging.Value = false;
            _dragDropped.OnNext(target);
        }

        public void CancelDrag()
        {
            if (_stateMachine.Phase == DragPhase.Idle) return;
            _stateMachine.Cancel();
            _isDragging.Value = false;
            _dragCancelled.OnNext(Unit.Default);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) CancelDrag();
        }

        private DropTarget ResolveDropTarget(Vector2 screenPos)
        {
            if (_gridCamera == null) return DropTarget.None();

            var ray = _gridCamera.ScreenPointToRay(screenPos);
            if (!Physics.Raycast(ray, out RaycastHit hit, RaycastMaxDistance, _raycastLayerMask))
                return DropTarget.None();

            var dropZone = hit.collider.GetComponent<MergeDropZone>();
            if (dropZone != null)
                return dropZone.ZoneType switch
                {
                    DropZoneType.SellZone => DropTarget.Sell(),
                    DropZoneType.TankSlot => DropTarget.Tank(),
                    _ => DropTarget.None()
                };

            if (_gridView != null && _gridView.TryWorldToGridPosition(hit.point, out int col, out int row))
                return DropTarget.GridCell(col, row);

            return DropTarget.None();
        }

        private bool TryGetGridCellFromScreenPos(Vector2 screenPos, out int col, out int row)
        {
            col = -1;
            row = -1;

            if (_gridCamera == null || _gridView == null) return false;

            var ray = _gridCamera.ScreenPointToRay(screenPos);
            if (!Physics.Raycast(ray, out RaycastHit hit, RaycastMaxDistance, _raycastLayerMask))
                return false;

            if (hit.collider.GetComponent<MergeDropZone>() != null) return false;

            return _gridView.TryWorldToGridPosition(hit.point, out col, out row);
        }

        private void OnDestroy()
        {
            _dragStarted.Dispose();
            _dragMoved.Dispose();
            _dragDropped.Dispose();
            _dragCancelled.Dispose();
            _cellTapped.Dispose();
            _isDragging.Dispose();
        }
    }
}