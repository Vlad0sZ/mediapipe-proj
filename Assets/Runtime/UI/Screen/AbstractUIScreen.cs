using Runtime.UI.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace Runtime.UI
{
    public abstract class AbstractUIScreen : MonoBehaviour, IScreen
    {
        [SerializeField] private string screenName;
        [SerializeField] private UnityEvent<bool> visibleChanged;
        
        public string ScreenName => screenName;
        
        protected void Awake() => Hide(true);

        public abstract void Show(bool instantly = false);

        public abstract void Hide(bool instantly = false);

        protected void RaiseVisibilityEvent(bool isVisible) => visibleChanged?.Invoke(isVisible);
    }
}