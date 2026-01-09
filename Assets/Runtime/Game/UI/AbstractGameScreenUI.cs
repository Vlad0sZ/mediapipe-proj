using Runtime.UI;
using UnityEngine;

namespace Runtime.Game.UI
{
    [RequireComponent(typeof(AbstractUIScreen))]
    public abstract class AbstractGameScreenUI : MonoBehaviour
    {
        [SerializeField] private AbstractUIScreen screen;

        protected virtual void OnValidate()
        {
            if (screen == null)
                screen = gameObject.GetComponent<AbstractUIScreen>();
        }

        protected virtual void OnEnable()
        {
            screen.OnBecameVisibleChanged.AddListener(OnVisibleScreenChange);
            screen.OnVisibleChanged.AddListener(OnVisibleScreenChanged);
        }

        protected virtual void OnDisable()
        {
            screen.OnBecameVisibleChanged.RemoveListener(OnVisibleScreenChange);
            screen.OnVisibleChanged.RemoveListener(OnVisibleScreenChanged);
        }

        private void OnVisibleScreenChange(bool visible)
        {
            if (visible)
                OnScreenShowing();
            else
                OnScreenHiding();
        }

        private void OnVisibleScreenChanged(bool visible)
        {
            if (visible)
                OnScreenShown();
            else
                OnScreenHidden();
        }

        protected virtual void OnScreenShowing()
        {
        }

        protected virtual void OnScreenHiding()
        {
        }

        protected virtual void OnScreenShown()
        {
        }

        protected virtual void OnScreenHidden()
        {
        }
    }
}