using Runtime.Game.Interfaces;
using UnityEngine;
using VContainer;

namespace Runtime.Game
{
    public abstract class ObjectSpawnerOwner : MonoBehaviour
    {
        protected IObjectSpawner ObjectSpawner { get; private set; }

        [Inject]
        public virtual void Construct(IObjectSpawner objectSpawner) =>
            ObjectSpawner = objectSpawner;
    }
}