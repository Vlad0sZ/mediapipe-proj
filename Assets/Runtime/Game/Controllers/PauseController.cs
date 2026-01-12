using System;
using JetBrains.Annotations;
using R3;
using Runtime.Game.Interfaces;
using Runtime.Types;
using UnityEngine;

namespace Runtime.Game.Controllers
{
    [UsedImplicitly]
    public sealed class PauseController : IPauseController
    {
        private readonly Subject<bool> _pauseSubject = new();
        private readonly IPlayerRaisePublisher _raisePublisher;
        private IDisposable _disposable;
        private bool _isPaused;
        private float _timeToRaise;

        public Observable<bool> OnPaused => _pauseSubject;

        public bool Paused
        {
            get => _isPaused;
            private set
            {
                if (_isPaused == value)
                    return;
                _isPaused = value;
                _pauseSubject.OnNext(value);
            }
        }

        public PauseController(IPlayerRaisePublisher raisePublisher) =>
            _raisePublisher = raisePublisher;

        public void StartControl()
        {
            _isPaused = false;
            _disposable = _raisePublisher.PlayerEvent.Subscribe(OnPlayerEvent);
        }

        public void StopControl() =>
            _disposable?.Dispose();

        private void OnPlayerEvent(PlayerPose playerPose)
        {
            var time = UpdateTime(playerPose);
            var paused = time >= 0f;
            var abs = Mathf.Abs(time);

            if (abs > 2f)
                Paused = paused;
        }


        private float UpdateTime(PlayerPose playerPose)
        {
            if (playerPose.IsVisible == false && _isPaused == false)
                _timeToRaise += Time.deltaTime;
            else if (playerPose.HandRaiseType == HandRaiseType.HandsRaised && _isPaused)
                _timeToRaise -= Time.deltaTime;
            else
                _timeToRaise = 0f;

            return _timeToRaise;
        }
    }
}