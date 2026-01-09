using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Runtime.UI
{
    public class ToggleListener : MonoBehaviour
    {
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private List<Toggle> toggles;
        [SerializeField] private UnityEvent<int> onValueChanged;

        public UnityEvent<int> OnValueChanged => onValueChanged;

        private void OnEnable()
        {
            foreach (var toggle in toggles)
            {
                if (!toggle)
                    continue;

                toggleGroup.RegisterToggle(toggle);
                toggle.onValueChanged.AddListener(ToggleWasChanged);
            }
        }

        private void OnDisable()
        {
            foreach (var toggle in toggles)
            {
                if (!toggle)
                    continue;

                toggleGroup.UnregisterToggle(toggle);
                toggle.onValueChanged.RemoveListener(ToggleWasChanged);
            }
        }

        private void ToggleWasChanged(bool value)
        {
            if (!value)
                return;

            var activeToggle = toggleGroup.GetFirstActiveToggle();
            var index = toggles.IndexOf(activeToggle);
            onValueChanged?.Invoke(index);
        }

        public void SetToggleOn(int index, bool withNotify = false)
        {
            if (index < 0 || index >= toggles.Count)
                return;

            var toggle = toggles[index];
            toggleGroup.NotifyToggleOn(toggle, withNotify);
        }
    }
}