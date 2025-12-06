using System;
using R3;
using Runtime.Game.Interfaces;
using UnityEngine;

namespace Runtime.Game
{
    public class CollectableItem : MonoBehaviour, ICollectableItem, ILifetimeItem
    {
        private readonly Subject<ICollectableItem> _collectableSubject = new Subject<ICollectableItem>();
        private readonly Subject<ILifetimeItem> _destructSubject = new Subject<ILifetimeItem>();

        private float _lifetime;
        private float _deadTime;

        public Observable<ICollectableItem> CollectableSubject => _collectableSubject;

        public Observable<ILifetimeItem> LifetimeObservable => _destructSubject;

        public int Points { get; set; }

        private void OnEnable() =>
            _lifetime = 0;

        private void OnDisable() =>
            _deadTime = 0;

        public void Collect() =>
            _collectableSubject.OnNext(this);

        public void SetLifetime(float seconds) =>
            _deadTime = seconds;

        private void OnDestroy() =>
            IsDisposed = true;

        private void Update()
        {
            _lifetime += Time.deltaTime;
            if (_deadTime == 0)
                return;

            if (_lifetime <= _deadTime)
                return;

            _deadTime = 0;
            _destructSubject.OnNext(this);
        }

        public bool IsDisposed { get; private set; }
    }
}