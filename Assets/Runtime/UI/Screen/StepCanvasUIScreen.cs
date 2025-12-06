using DG.Tweening;
using UnityEngine;

namespace Runtime.UI
{
    public class StepCanvasUIScreen : AbstractUIScreen
    {
        [SerializeField] private CanvasGroup selfCanvasGroup;
        [SerializeField] private CanvasGroup[] childrenCanvasGroup;
        [SerializeField] private float duration = 0.33f;
        [SerializeField] private float delay = 0.12f;
        private Tween _tween;

        public override void Show(bool instantly = false) =>
            ChangeVisible(true, instantly);

        public override void Hide(bool instantly = false) =>
            ChangeVisible(false, instantly);

        private void ChangeVisible(bool isVisible, bool instantly)
        {
            _tween?.Kill();

            if (instantly)
            {
                ChangeAllCanvas(isVisible);
            }
            else
            {
                _tween = GetTween(isVisible);
                _tween.Play();
            }
        }

        private Tween GetTween(bool isVisible)
        {
            var targetAlpha = isVisible ? 1f : 0f;

            var sequence = DOTween.Sequence();
            sequence.Append(selfCanvasGroup.DOFade(targetAlpha, duration));
            sequence.AppendInterval(duration);
            foreach (var child in childrenCanvasGroup)
            {
                sequence.Append(child.DOFade(targetAlpha, duration));
                sequence.AppendInterval(delay);
            }


            if (isVisible)
                sequence.OnComplete(() => ChangeAllCanvas(true));
            else
                sequence.OnStart(() => ChangeAllCanvas(false));

            return sequence;
        }


        private void ChangeAllCanvas(bool isVisible)
        {
            ChangeVisible(selfCanvasGroup, isVisible);
            foreach (var canvasGroup in childrenCanvasGroup)
                ChangeVisible(canvasGroup, isVisible);

            RaiseVisibilityEvent(isVisible);
        }

        private static void ChangeVisible(CanvasGroup canvasGroup, bool isVisible)
        {
            canvasGroup.alpha = isVisible ? 1f : 0f;
            canvasGroup.interactable = isVisible;
            canvasGroup.blocksRaycasts = isVisible;
        }
    }
}