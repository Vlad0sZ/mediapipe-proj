using Runtime.UI.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace Runtime.UI
{
    public abstract class AbstractUIScreen : MonoBehaviour, IScreen
    {
        [SerializeField] private string screenName;
        [SerializeField] private UnityEvent<bool> visibleChanged;
        [SerializeField] private UnityEvent<bool> onBecameVisibleChanged;
        public UnityEvent<bool> OnVisibleChanged => visibleChanged;

        public UnityEvent<bool> OnBecameVisibleChanged => onBecameVisibleChanged;

        public string ScreenName => screenName;

        protected void Awake() => Hide(true);

        public abstract void Show(bool instantly = false, System.Action callback = null);

        public abstract void Hide(bool instantly = false, System.Action callback = null);

        protected void RaiseVisibilityEvent(bool isVisible) => visibleChanged?.Invoke(isVisible);

        protected void RaiseBecameVisibilityEvent(bool isVisible) => onBecameVisibleChanged?.Invoke(isVisible);
    }
}