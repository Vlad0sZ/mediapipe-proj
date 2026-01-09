using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Runtime.UI
{
    public class StepCanvasUIScreen : AbstractUIScreen
    {
        [SerializeField] private CanvasGroup selfCanvasGroup;
        [SerializeField] private CanvasGroup[] childrenCanvasGroup;
        [SerializeField] private float showDuration = 0.33f;
        [SerializeField] private float hideDuration = 0.33f;
        [SerializeField] private float showDelay = 0.12f;
        [SerializeField] private float hideDelay = 0.12f;
        private Tween _tween;

        public override void Show(bool instantly = false, System.Action callback = null) =>
            ChangeVisible(true, instantly, callback);

        public override void Hide(bool instantly = false, System.Action callback = null) =>
            ChangeVisible(false, instantly, callback);

        private void ChangeVisible(bool isVisible, bool instantly, System.Action callback = null)
        {
            _tween?.Kill();

            RaiseBecameVisibilityEvent(isVisible);

            if (instantly)
            {
                ChangeAllCanvas(isVisible, isVisible);
                RaiseVisibilityEvent(isVisible);
                callback?.Invoke();
            }
            else
            {
                _tween = GetTween(isVisible);
                _tween.onComplete += () => callback?.Invoke();
                _tween.Play();
            }
        }

        private Tween GetTween(bool isVisible)
        {
            var targetAlpha = isVisible ? 1f : 0f;

            var sequence = DOTween.Sequence();
            var allCanvases = new List<CanvasGroup>() {selfCanvasGroup}.Union(childrenCanvasGroup);
            if (!isVisible)
                allCanvases = allCanvases.Reverse();


            var duration = isVisible ? showDuration : hideDuration;
            var delay = isVisible ? showDelay : hideDelay;

            foreach (var canvasGroup in allCanvases)
            {
                sequence.Append(canvasGroup.DOFade(targetAlpha, duration));
                sequence.AppendInterval(delay);
            }

            if (!isVisible)
                sequence.OnStart(() => ChangeAllCanvas(true, false));

            sequence.onComplete += () => ChangeAllCanvas(isVisible, isVisible);
            sequence.onComplete += () => RaiseVisibilityEvent(isVisible);
            return sequence;
        }


        private void ChangeAllCanvas(bool isVisible, bool activated)
        {
            ChangeVisible(selfCanvasGroup, isVisible, activated);

            foreach (var canvasGroup in childrenCanvasGroup)
                ChangeVisible(canvasGroup, isVisible, activated);
        }

        private static void ChangeVisible(CanvasGroup canvasGroup, bool isVisible, bool activated)
        {
            canvasGroup.alpha = isVisible ? 1f : 0f;
            canvasGroup.interactable = activated;
            canvasGroup.blocksRaycasts = activated;
        }
    }
}