using UnityEngine;

namespace Game.Merge
{
    [RequireComponent(typeof(Collider))]
    public class MergeDropZone : MonoBehaviour
    {
        [SerializeField] private DropZoneType _zoneType;

        public DropZoneType ZoneType => _zoneType;
    }
}