using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.Game.UI
{
    public sealed class SliderSubText : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text subText;

        public IFormatter Formatter { get; set; }

        private void OnEnable() =>
            slider.onValueChanged.AddListener(OnSliderChanged);

        private void OnDisable() =>
            slider.onValueChanged.RemoveListener(OnSliderChanged);

        private void OnSliderChanged(float value)
        {
            var str = Formatter?.Format(value) ?? $"{value:F1}";
            subText.text = str;
        }

        public interface IFormatter
        {
            string Format(float clampedValue);
        }
    }
}