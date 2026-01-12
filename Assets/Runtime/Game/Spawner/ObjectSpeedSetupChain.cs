using Runtime.Game.Interfaces;
using Runtime.Game.ScriptableData;
using UnityEngine;

namespace Runtime.Game.Spawner
{
    public sealed class ObjectSpeedSetupChain : ISpawnerChain
    {
        private readonly GameSettings.ObjectsSettings _speedSettings;

        public ObjectSpeedSetupChain(GameSettings.ObjectsSettings speedSettings) =>
            _speedSettings = speedSettings;

        public void OnSpawned(GameObject gameObject)
        {
            var fallComponent = gameObject.GetComponent<IFallComponentSetup>();
            fallComponent.Setup(_speedSettings);
        }

        public void OnReleased(GameObject gameObject)
        {
        }
    }
}