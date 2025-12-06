using System;
using R3;
using Runtime.Game.Timers;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Runtime.Game.UI
{
    public sealed class TimerUI : MonoBehaviour
    {
        [SerializeField] private Image filledImage;
        private ITimerPublisher _timer;
        private IDisposable _actionDisposable;

        [Inject]
        public void Construct(ITimerPublisher timer) => _timer = timer;


        private void OnEnable() => _actionDisposable = _timer.Event.Subscribe(OnTimerEvent);
        private void OnDisable() => _actionDisposable?.Dispose();


        private void OnTimerEvent(ElapsedTime elapsedTime)
        {
            var p = elapsedTime.Progress;
            filledImage.fillAmount = p;
        }
    }
}