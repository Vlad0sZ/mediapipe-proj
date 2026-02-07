using System.Collections.Generic;
using Runtime.Game.Interfaces;
using UnityEngine;

namespace Runtime.Game.Factories
{
    public interface IPlayerFactory
    {
        void SpawnPlayer(Vector3 position);
        void RemovePlayer();

        GameObject GetPlayer();

        void SetupPlayer(Dictionary<string, int> skinSettings);
    }
}