using UnityEngine;

namespace Game.Merge
{
    public sealed class DragStateMachine
    {
        public DragPhase Phase { get; private set; } = DragPhase.Idle;
        public Vector2Int SourceCell { get; private set; }
        public Vector2 PressOrigin { get; private set; }
        public bool HasPartAtSource { get; private set; }

        public bool TryStartPress(Vector2Int cell, Vector2 screenPos, bool hasPart)
        {
            if (Phase != DragPhase.Idle) return false;
            Phase = DragPhase.Pressing;
            SourceCell = cell;
            PressOrigin = screenPos;
            HasPartAtSource = hasPart;
            return true;
        }

        public bool TryEscalateToDrag(Vector2 currentPos, float threshold)
        {
            if (Phase != DragPhase.Pressing) return false;
            if (Vector2.Distance(currentPos, PressOrigin) <= threshold) return false;
            Phase = DragPhase.Dragging;
            return true;
        }

        public void Release() => Phase = DragPhase.Idle;
        public void Cancel() => Phase = DragPhase.Idle;
    }
}