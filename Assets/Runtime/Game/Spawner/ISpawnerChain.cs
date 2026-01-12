using UnityEngine;

namespace Runtime.Game.Spawner
{
    public interface ISpawnerChain
    {
        void OnSpawned(GameObject gameObject);

        void OnReleased(GameObject gameObject);
    }
}