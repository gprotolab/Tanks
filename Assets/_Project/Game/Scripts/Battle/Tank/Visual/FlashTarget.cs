using UnityEngine;

namespace Game.Battle
{
    public class FlashTarget : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _renderer;

        public MeshRenderer Renderer => _renderer;
    }
}