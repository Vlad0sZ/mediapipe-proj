using System;
using Runtime.Game.Interfaces;
using UnityEngine;
using VContainer;

namespace Runtime.Game
{
    public abstract class ObjectSpawnerOwner : MonoBehaviour
    {
        [SerializeField] private int order;
        public virtual int Order => order;
        public abstract void Configure(IObjectSpawner objectSpawner);
        public abstract void Deconstruct();
    }
}