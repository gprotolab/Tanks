using UnityEngine;

namespace Game.Merge
{
    [CreateAssetMenu(fileName = "MergeConfig", menuName = "Configs/MergeConfig")]
    public class MergeConfigSO : ScriptableObject
    {
        [SerializeField] private MergeSettings _normalSession = new();
        [SerializeField] private MergeSettings _tutorialSession = new();

        public MergeSettings NormalSession => _normalSession;

        public MergeSettings TutorialSession => _tutorialSession;
    }
}