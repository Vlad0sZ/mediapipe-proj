using System;
using System.Collections.Generic;
using R3;
using Runtime.Game.Interfaces;
using UnityEngine;

namespace Runtime.Game
{
    public class ObjectSetup : ObjectSpawnerOwner
    {
        [SerializeField] private float objectLifetime;

        private IDisposable _disposable;
        private readonly Dictionary<GameObject, IDisposable> _objectSubscriptions = new();

        private void OnEnable() =>
            _disposable = ObjectSpawner.OnObjectSpawned.Subscribe(SetupObject);

        private void OnDisable() =>
            _disposable?.Dispose();

        private void SetupObject(ObjectSpawner.SpawnEvent spawnEvent)
        {
            if (spawnEvent is not ObjectSpawner.SpawnEvent.Created createdEvent)
                return;

            var obj = createdEvent.Object;
            var collectableItem = obj.GetComponent<ICollectableItem>();
            var lifetimeItem = obj.GetComponent<ILifetimeItem>();


            var collectableDisposable = collectableItem?.CollectableSubject.Subscribe(SubscribeToRelease) ?? default;
            var lifetimeDisposable = lifetimeItem?.LifetimeObservable.Subscribe(SubscribeToRelease) ?? default;
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
            _objectSubscriptions.GetValueOrDefault(obj.gameObject)?.Dispose();
            _objectSubscriptions.Remove(obj.gameObject);
            ObjectSpawner.ReleaseObject(obj.gameObject);
        }
    }
}