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
            ObjectsSettings ObjectsSettings);

        [System.Serializable]
        public struct SpawnSettings
        {
            public int maxObjectPerSpawn;
            public float spawnDelay;
        }

        [System.Serializable]
        public struct LevelSettings
        {
            public float levelTime;
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
            var level = levelSettings[Mathf.Min(byLevel, levelSettings.Length)];
            var objectsSetting = objectsSettings[Mathf.Min(byLevel, objectsSettings.Length)];

            return new Settings(level, spawn, objectsSetting);
        }
    }
}