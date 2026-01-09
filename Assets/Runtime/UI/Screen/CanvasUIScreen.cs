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
        public override void Show(bool instantly = false, System.Action callback = null) =>
            RestartTween(true, instantly, callback);

        [ContextMenu(nameof(Hide))]
        public override void Hide(bool instantly = false, System.Action callback = null) =>
            RestartTween(false, instantly, callback);

        private void RestartTween(bool isShow, bool instantly, System.Action callback)
        {
            _tween?.Kill();

            RaiseBecameVisibilityEvent(isShow);

            if (instantly)
            {
                ChangeCanvasGroup(isShow, isShow);
                RaiseVisibilityEvent(isShow);
                callback?.Invoke();
            }
            else
            {
                _tween = AnimateTo(isShow);
                _tween.onComplete += () => callback?.Invoke();
                _tween.Play();
            }
        }

        private Tween AnimateTo(bool isShow)
        {
            var alpha = isShow ? 1f : 0f;
            var tween = canvasGroup.DOFade(alpha, duration);

            if (isShow == false)
                tween.OnStart(() => ChangeCanvasGroup(true, false));

            tween.onComplete += () => ChangeCanvasGroup(isShow, isShow);
            tween.onComplete += () => RaiseVisibilityEvent(isShow);
            return tween;
        }

        private void ChangeCanvasGroup(bool isVisible, bool activated)
        {
            canvasGroup.alpha = isVisible ? 1f : 0f;
            canvasGroup.interactable = activated;
            canvasGroup.blocksRaycasts = activated;
        }


        private void OnValidate()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }
    }
}