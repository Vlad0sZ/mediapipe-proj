using System;
using DG.Tweening;
using Runtime.UI.Interfaces;
using UnityEngine;

namespace Runtime.Game.UI
{
    public sealed class HintScreen : MonoBehaviour, IScreen
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeTime;

        private Tween _tween;

        public void Show(bool instantly = false, Action callback = null) =>
            SetupAnimation(true, instantly, callback);

        public void Hide(bool instantly = false, Action callback = null) =>
            SetupAnimation(false, instantly, callback);

        private void SetupAnimation(bool visible, bool instantly = false, Action callback = null)
        {
            _tween?.Kill();

            if (instantly)
            {
                SetCanvasGroup(visible);
                callback?.Invoke();
            }
            else
            {
                var a = visible ? 1f : 0f;
                _tween = canvasGroup.DOFade(a, fadeTime);
                _tween.onComplete += () => SetCanvasGroup(visible);
                _tween.onComplete += () => callback?.Invoke();
                _tween.Play();
            }
        }

        private void SetCanvasGroup(bool visible)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
        }
    }
}