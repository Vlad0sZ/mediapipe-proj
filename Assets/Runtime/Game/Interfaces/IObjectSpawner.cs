using R3;
using Runtime.Game.ScriptableData;
using UnityEngine;

namespace Runtime.Game.Interfaces
{
    public interface IObjectSpawner :  ISetupPayload<GameSettings.SpawnSettings>
    {
        Observable<ObjectSpawner.SpawnEvent> OnObjectSpawned { get; }
        
        Observable<bool> OnSpawnProcess { get; }
        
        void ReleaseObject(GameObject releaseObject);

        void StartSpawn();

        void StopSpawn();
    }
}