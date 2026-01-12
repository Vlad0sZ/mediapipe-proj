using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Runtime.Game.Interfaces;
using Runtime.Game.Spawner;
using UnityEngine;

namespace Runtime.Game.TrainingCustoms
{
    public class TrainingObjectChain : ISpawnerChain, IDisposable
    {
        private readonly GameObject _prefab;
        private readonly UniTaskCompletionSource<ICollectableItem> _tcs;
        private readonly Dictionary<GameObject, GameObject> _parents = new();

        public TrainingObjectChain(GameObject prefab, UniTaskCompletionSource<ICollectableItem> tcs)
        {
            _prefab = prefab;
            _tcs = tcs;
        }

        public void OnSpawned(GameObject gameObject)
        {
            if (_parents.TryGetValue(gameObject, out var g) && g != null)
                UnityEngine.Object.Destroy(g);

            if(gameObject.TryGetComponent<ILifetimeItem>(out var lifetimeItem))
                lifetimeItem.SetLifetime(0);
            
            var instance = UnityEngine.Object.Instantiate(_prefab, gameObject.transform, false);
            instance.transform.localPosition = Vector3.zero;
            _parents[gameObject] = instance;
            _tcs.TrySetResult(gameObject.GetComponent<ICollectableItem>());
        }

        public void OnReleased(GameObject gameObject)
        {
            if (_parents.TryGetValue(gameObject, out var g) && g != null)
                UnityEngine.Object.Destroy(g);
        }

        public void Dispose()
        {
            foreach (var pair in _parents)
                if (pair.Value != null)
                    UnityEngine.Object.Destroy(pair.Value);

            _parents.Clear();
        }
    }
}