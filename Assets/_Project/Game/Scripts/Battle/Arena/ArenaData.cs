using UnityEngine;

namespace Game.Battle
{
    public class ArenaData
    {
        public GameObject Instance { get; }

        public Transform[] FfaSpawnPoints { get; }

        public Transform[] TeamASpawnPoints { get; }

        public Transform[] TeamBSpawnPoints { get; }

        // Keep the address so the loaded arena can be released later.
        public string Address { get; }

        public ArenaData(
            GameObject instance,
            Transform[] ffaSpawnPoints,
            Transform[] teamASpawnPoints,
            Transform[] teamBSpawnPoints,
            string address)
        {
            Instance = instance;
            FfaSpawnPoints = ffaSpawnPoints;
            TeamASpawnPoints = teamASpawnPoints;
            TeamBSpawnPoints = teamBSpawnPoints;
            Address = address;
        }
    }
}