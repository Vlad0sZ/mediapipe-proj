using System;
using System.Collections.Generic;
using R3;
using Runtime.Game.Interfaces;
using Runtime.Game.Publishers;
using UnityEngine;

namespace Runtime.Game
{
    public class ObjectCollectSetup : ObjectSpawnerOwner, IScorePublisher
    {
        [SerializeField] private int score;

        private readonly Dictionary<GameObject, IDisposable> _subscriptions = new();
        private readonly Subject<ScoreModel> _scoreSubject = new();
        private IDisposable _disposable;

        public ScoreModel Score => new ScoreModel(score);
        public Observable<ScoreModel> OnScore => _scoreSubject;

        private void OnEnable()
        {
            score = 0;

            var createSub = ObjectSpawner.OnObjectSpawned
                .Where(u => u is ObjectSpawner.SpawnEvent.Created)
                .Select(u => u.Object)
                .Subscribe(SubscribeToCollect);

            var releaseSub = ObjectSpawner.OnObjectSpawned
                .Where(u => u is ObjectSpawner.SpawnEvent.Released)
                .Select(u => u.Object)
                .Subscribe(SubscribeToRelease);

            var spawnSub = ObjectSpawner.OnSpawnProcess
                .Subscribe(SubscribeToSpawnProcess);

            _disposable = Disposable.Combine(createSub, releaseSub, spawnSub);
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

        private void SubscribeToSpawnProcess(bool isSpawningNow)
        {
            if (isSpawningNow)
                score = 0;
        }

        private void Collect(ICollectableItem collectable)
        {
            score += collectable.Points;
            _scoreSubject.OnNext(new ScoreModel(score));
        }
    }
}