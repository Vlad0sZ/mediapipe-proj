using System.Linq;
using Runtime.Game.Interfaces;
using UnityEngine;
using VContainer;

namespace Runtime.Game
{
    public sealed class ObjectSpawnerSetup : MonoBehaviour
    {
        [SerializeField] private ObjectSpawnerOwner[] owners;

        [Inject] private IObjectSpawner ObjectSpawner { get; set; }

        private void OnEnable()
        {
            foreach (var objectSpawnerOwner in owners.OrderBy(x => x.Order))
                objectSpawnerOwner.Configure(ObjectSpawner);
        }

        private void OnDisable()
        {
            foreach (var objectSpawnerOwner in owners.OrderBy(x => x.Order))
                objectSpawnerOwner.Deconstruct();
        }
    }
}