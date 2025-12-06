using DG.Tweening;
using UnityEngine;

namespace Runtime.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasUIScreen : AbstractUIScreen
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float duration = 0.33f;
        private Tween _tween;

        [ContextMenu(nameof(Show))]
        public override void Show(bool instantly = false) =>
            RestartTween(true, instantly);

        [ContextMenu(nameof(Hide))]
        public override void Hide(bool instantly = false) =>
            RestartTween(false, instantly);

        private void RestartTween(bool isShow, bool instantly)
        {
            _tween?.Kill();

            if (instantly)
            {
                ChangeCanvasGroup(isShow);
                canvasGroup.alpha = isShow ? 1f : 0f;
            }
            else
            {
                _tween = AnimateTo(isShow);
                _tween.Play();
            }
        }

        private Tween AnimateTo(bool isShow)
        {
            var alpha = isShow ? 1f : 0f;
            var tween = canvasGroup.DOFade(alpha, duration);

            if (isShow)
                tween.OnComplete(() => ChangeCanvasGroup(true));
            else
                tween.OnStart(() => ChangeCanvasGroup(false));

            return tween;
        }

        private void ChangeCanvasGroup(bool activated)
        {
            canvasGroup.interactable = activated;
            canvasGroup.blocksRaycasts = activated;
            RaiseVisibilityEvent(activated);
        }


        private void OnValidate()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }
    }
}