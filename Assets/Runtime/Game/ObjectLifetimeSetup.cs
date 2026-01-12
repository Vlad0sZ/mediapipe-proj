using System;
using System.Collections.Generic;
using R3;
using Runtime.Game.Interfaces;
using UnityEngine;

namespace Runtime.Game
{
    public sealed class ObjectLifetimeSetup : ObjectSpawnerOwner
    {
        [SerializeField] private float objectLifetime;
        private readonly Dictionary<GameObject, IDisposable> _objectSubscriptions = new();
        private IDisposable _disposable;
        private IObjectSpawner _objectSpawner;

        public override void Configure(IObjectSpawner objectSpawner)
        {
            _objectSpawner = objectSpawner;
            _disposable = objectSpawner.OnObjectSpawned.Subscribe(SetupObject);
        }

        public override void Deconstruct() =>
            _disposable?.Dispose();

        private void SetupObject(ObjectSpawner.SpawnEvent spawnEvent)
        {
            if (spawnEvent is not ObjectSpawner.SpawnEvent.Created createdEvent)
                return;

            var obj = createdEvent.Object;
            var lifetimeItem = obj.GetComponent<ILifetimeItem>();
            var collectableItem = obj.GetComponent<ICollectableItem>();

            var collectableDisposable = collectableItem?.CollectableSubject.Subscribe(SubscribeToCollect);
            var lifetimeDisposable = lifetimeItem?.LifetimeObservable.Subscribe(SubscribeToRelease);
            lifetimeItem?.SetLifetime(objectLifetime);

            var builder = Disposable.CreateBuilder();

            if (collectableDisposable != null)
                builder.Add(collectableDisposable);

            if (lifetimeDisposable != null)
                builder.Add(lifetimeDisposable);

            _objectSubscriptions[obj] = builder.Build();
        }

        private void SubscribeToRelease(IGameObject obj)
        {
            SubscribeToCollect(obj);
            _objectSpawner.ReleaseObject(obj.gameObject);
        }

        private void SubscribeToCollect(IGameObject obj)
        {
            _objectSubscriptions.GetValueOrDefault(obj.gameObject)?.Dispose();
            _objectSubscriptions.Remove(obj.gameObject);
        }

        private void OnDestroy()
        {
            foreach (var subs in _objectSubscriptions)
                subs.Value?.Dispose();

            _objectSubscriptions.Clear();
        }
    }
}