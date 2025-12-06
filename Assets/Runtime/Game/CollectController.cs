using System;
using System.Collections.Generic;
using R3;
using Runtime.Game.Interfaces;
using UnityEngine;

namespace Runtime.Game
{
    public class CollectController : ObjectSpawnerOwner
    {
        [SerializeField] private int score;

        private readonly Dictionary<GameObject, IDisposable> _subscriptions = new();
        private IDisposable _disposable;

        private void OnEnable()
        {
            var createSub = ObjectSpawner.OnObjectSpawned
                .Where(u => u is ObjectSpawner.SpawnEvent.Created)
                .Select(u => u.Object)
                .Subscribe(SubscribeToCollect);

            var releaseSub = ObjectSpawner.OnObjectSpawned
                .Where(u => u is ObjectSpawner.SpawnEvent.Released)
                .Select(u => u.Object)
                .Subscribe(SubscribeToRelease);

            _disposable = Disposable.Combine(createSub, releaseSub);
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }


        private void SubscribeToCollect(GameObject obj)
        {
            if (obj.TryGetComponent<ICollectableItem>(out var item) == false)
                return;

            var disposable = item.CollectableSubject.Subscribe(Collect);
            _subscriptions[obj] = disposable;
        }

        private void SubscribeToRelease(GameObject obj)
        {
            if (_subscriptions.TryGetValue(obj, out var d))
                d?.Dispose();
        }

        private void Collect(ICollectableItem collectable)
        {
            score += collectable.Points;
            UnityEngine.Debug.Log($"Update score (+{collectable.Points}), score = {score}");
        }
    }
}