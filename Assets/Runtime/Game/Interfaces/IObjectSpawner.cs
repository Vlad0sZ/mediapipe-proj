using R3;
using Runtime.Game.ScriptableData;
using UnityEngine;

namespace Runtime.Game.Interfaces
{
    public interface IObjectSpawner 
    {
        Observable<ObjectSpawner.SpawnEvent> OnObjectSpawned { get; }
        
        Observable<bool> OnSpawnProcess { get; }
        void Configure(GameSettings.SpawnSettings spawnSettings);
        void ReleaseObject(GameObject releaseObject);

        void StartSpawn();

        void StopSpawn();

        void Pause();

        void Resume();
    }
}