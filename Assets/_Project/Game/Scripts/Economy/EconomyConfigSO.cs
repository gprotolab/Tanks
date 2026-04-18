using Newtonsoft.Json;
using UnityEngine;

namespace Game.Economy
{
    [CreateAssetMenu(fileName = "EconomyConfig", menuName = "Configs/Economy")]
    public class EconomyConfigSO : ScriptableObject
    {
        [SerializeField] private EconomySettings _settings = new();

        public EconomySettings Defaults => _settings;

        public EconomySettings CreateRuntimeCopy()
        {
            string json = JsonConvert.SerializeObject(_settings);
            return JsonConvert.DeserializeObject<EconomySettings>(json);
        }
    }
}