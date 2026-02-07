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
        public ParticleSystem successParticles;
        public ParticleSystem failureParticles;

        private readonly Dictionary<GameObject, IDisposable> _subscriptions = new();
        private readonly Subject<ScoreModel> _scoreSubject = new();
        private IDisposable _disposable;
        private IObjectSpawner _objectSpawner;

        private ScoreModel _scoreModel;

        public ScoreModel Score
        {
            get => _scoreModel;

            private set
            {
                _scoreModel = value;
                _scoreSubject.OnNext(_scoreModel);
            }
        }

        public Observable<ScoreModel> OnScore => _scoreSubject;

        public override void Configure(IObjectSpawner objectSpawner)
        {
            _objectSpawner = objectSpawner;

            var createSub = objectSpawner.OnObjectSpawned
                .Where(u => u is ObjectSpawner.SpawnEvent.Created)
                .Select(u => u.Object)
                .Subscribe(SubscribeToCollect);

            var releaseSub = objectSpawner.OnObjectSpawned
                .Where(u => u is ObjectSpawner.SpawnEvent.Released)
                .Select(u => u.Object)
                .Subscribe(SubscribeToRelease);

            var spawnSub = objectSpawner.OnSpawnProcess
                .Subscribe(SubscribeToSpawnProcess);

            _disposable = Disposable.Combine(createSub, releaseSub, spawnSub);
        }

        public override void Deconstruct() =>
            _disposable?.Dispose();


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
                Score = default;
        }

        private void Collect(ICollectableItem collectable)
        {
            var points = collectable.Points;
            var positive = Score.PositiveScore;
            var negative = Score.NegativeScore;

            if (points > 0)
            {
                if (successParticles)
                    Instantiate(successParticles, collectable.gameObject.transform.position, Quaternion.identity);
                positive += points;
            }
            else
            {
                if (failureParticles)
                    Instantiate(failureParticles, collectable.gameObject.transform.position, Quaternion.identity);
                negative += points;
            }

            Score = new ScoreModel(positive, negative);
            _objectSpawner.ReleaseObject(collectable.gameObject);
        }
    }
}