using UnityEngine;

namespace Runtime.Game.Factories
{
    public interface IPlayerFactory
    {
        void SpawnPlayer(Vector3 position);
        void RemovePlayer();
    }
}