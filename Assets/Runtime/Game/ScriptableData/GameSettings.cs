using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Game.ScriptableData
{
    [CreateAssetMenu(fileName = "Game Settings", menuName = "Game/Settings", order = 2)]
    public class GameSettings : ScriptableObject
    {
        [System.Serializable]
        public struct SpawnSettings
        {
            public float spawnDelay;
            public int maxObjectPerSpawn;
            public int maxObjects;
        }
        
        [System.Serializable]
        public struct LevelSettings
        {
            public float levelTime;
        }

        [FormerlySerializedAs("settings")] [SerializeField] private SpawnSettings[] spawnSettings;
        [SerializeField] private LevelSettings[] levelSettings;

        public SpawnSettings GetSpawnSettings(int byLevel) =>
            spawnSettings[Mathf.Min(byLevel, spawnSettings.Length)];
        
        public LevelSettings GetLevelSettings(int byLevel) => 
            levelSettings[Mathf.Min(byLevel, levelSettings.Length)];
    }
}