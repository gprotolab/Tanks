using UnityEngine;

namespace Game.Battle
{
    [CreateAssetMenu(fileName = "BotNamesList", menuName = "Configs/Battle/BotNamesList")]
    public class BotNamesConfigSO : ScriptableObject
    {
        [Tooltip("Comma-separated bot names")] [TextArea(5, 20)] [SerializeField]
        private string _namesText =
            "Shadow,IronFist,BlazeTank,NovaStrike,TitanRush,StormBolt,ViperX,EagleEye,Phoenix,Demolisher";

        private string[] _cachedNames;

        public string[] GetNames()
        {
            if (_cachedNames == null || _cachedNames.Length == 0)
            {
                _cachedNames = _namesText.Split(',', System.StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < _cachedNames.Length; i++)
                {
                    _cachedNames[i] = _cachedNames[i].Trim();
                }
            }

            return _cachedNames;
        }

#if UNITY_EDITOR
        private void OnValidate() => _cachedNames = null;
#endif
    }
}