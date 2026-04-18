using TMPro;
using UnityEngine;

namespace Game.Merge
{
    public class PartLevelLabel : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _label;

        public void SetLevel(int level)
        {
            if (_label == null) return;

            _label.text = $"{level}";
        }
    }
}