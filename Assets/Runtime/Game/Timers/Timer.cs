using JetBrains.Annotations;
using R3;
using Runtime.Game.ScriptableData;
using UnityEngine;

namespace Runtime.Game.Timers
{
    [UsedImplicitly]
    public sealed class Timer : ITimer
    {
        private readonly Subject<ElapsedTime> _progress = new();
        public Observable<ElapsedTime> Event => _progress;

        private float _totalTimes;
        private float _time;
        private bool _isRunning;
        private bool _isPaused;

        public void Setup(GameSettings.LevelSettings payload)
        {
            _totalTimes = payload.levelTime;
            _time = 0f;
        }

        public void StartTimer()
        {
            _isPaused = false;
            _isRunning = true;
            _time = 0f;
        }


        public void StopTimer()
        {
            _isPaused = false;
            _isRunning = false;
            _time = 0f;
        }

        public void Pause()
        {
            if (_isPaused == false)
                _isPaused = true;
        }

        public void Resume()
        {
            if (_isPaused)
                _isPaused = false;
        }

        public void Tick()
        {
            if (_isRunning == false || _isPaused)
                return;

            _time += Time.deltaTime;

            if (_totalTimes > _time)
            {
                var p = Mathf.Clamp01(_time / _totalTimes);
                _progress.OnNext(new ElapsedTime(_totalTimes, p));
            }
            else
            {
                _progress.OnNext(new ElapsedTime(_totalTimes, 1f));
                _isRunning = false;
            }
        }
    }
}