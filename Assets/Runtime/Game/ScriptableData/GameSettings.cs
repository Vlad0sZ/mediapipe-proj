using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Game.ScriptableData
{
    [CreateAssetMenu(fileName = "Game Settings", menuName = "Game/Settings", order = 2)]
    public class GameSettings : ScriptableObject
    {
        public record Settings(
            LevelSettings LevelSettings,
            SpawnSettings SpawnSettings,
            ObjectsSettings ObjectsSettings
        );

        [System.Serializable]
        public struct SpawnSettings
        {
            public int maxObjectPerSpawn;
            public float spawnDelay;
        }

        [System.Serializable]
        public struct LevelSettings
        {
            public float minLevelTime;
            public float maxLevelTime;

            public bool Endless() => minLevelTime == 0 && maxLevelTime == 0;
        }

        [System.Serializable]
        public struct ObjectsSettings
        {
            public Vector2 minMaxFallSpeed;
            public Vector2 minMaxRotationSpeed;
        }
        
        [FormerlySerializedAs("settings")] [SerializeField]
        private SpawnSettings[] spawnSettings;

        [SerializeField] private LevelSettings[] levelSettings;
        [SerializeField] private ObjectsSettings[] objectsSettings;


        public Settings GetSettings(int byLevel)
        {
            var spawn = spawnSettings[Mathf.Min(byLevel, spawnSettings.Length)];
            var objectsSetting = objectsSettings[Mathf.Min(byLevel, objectsSettings.Length)];
            var level = levelSettings.ElementAtOrDefault(byLevel);

            return new Settings(level, spawn, objectsSetting);
        }
    }
}