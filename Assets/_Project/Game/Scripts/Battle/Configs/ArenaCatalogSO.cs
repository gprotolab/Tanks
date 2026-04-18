using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Battle
{
    [CreateAssetMenu(fileName = "ArenaCatalog", menuName = "Configs/Battle/ArenaCatalog")]
    public class ArenaCatalogSO : ScriptableObject
    {
        [SerializeField] private ArenaEntry[] _arenas;

        public ArenaEntry[] Arenas => _arenas;

        [Serializable]
        public class ArenaEntry
        {
            [SerializeField] private AssetReferenceGameObject _prefabReference;

            [SerializeField] private bool _isActive = true;

            public AssetReferenceGameObject PrefabReference => _prefabReference;
            public bool IsActive => _isActive;
        }
    }
}