using System.Text;
using Runtime.Game.Interfaces;
using Runtime.Game.Types;
using Runtime.UI;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Runtime.Game.UI
{
    public sealed class GameModeSettingsUI : AbstractGameScreenUI, SliderSubText.IFormatter
    {
        [SerializeField] private ToggleListener gameModeGroup;
        [SerializeField] private Slider timeSlider;
        [SerializeField] private SliderSubText subText;

        private IGameModeSettings _gameSettings;

        [Inject]
        public void Construct(IGameModeSettings gameSettings) =>
            _gameSettings = gameSettings;

        private void Start() => subText.Formatter = this;

        protected override void OnScreenShowing()
        {
            base.OnScreenShown();
            _gameSettings.SetupGameMode(GameMode.Classic);
            _gameSettings.SetLevelTime(0.5f);
            gameModeGroup.SetToggleOn(1, true);
            timeSlider.value = 0.5f;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            gameModeGroup.OnValueChanged.AddListener(OnModeChanged);
            timeSlider.onValueChanged.AddListener(OnSliderChanged);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            gameModeGroup.OnValueChanged.RemoveListener(OnModeChanged);
            timeSlider.onValueChanged.RemoveListener(OnSliderChanged);
        }

        private void OnModeChanged(int index)
        {
            var gm = (GameMode) index;
            var time = gm == GameMode.Classic ? timeSlider.value : 0f;
            _gameSettings.SetupGameMode(gm);
            _gameSettings.SetLevelTime(time);
        }

        private void OnSliderChanged(float value) =>
            _gameSettings.SetLevelTime(value);

        public string Format(float clampedValue)
        {
            var levelTime = _gameSettings?.GetLevelTimeSettings() ?? default;
            if (levelTime.Endless())
                return string.Empty;

            float timeClamp = Mathf.Lerp(levelTime.minLevelTime, levelTime.maxLevelTime, clampedValue);
            int time = Mathf.RoundToInt(timeClamp);
            var minutes = time / 60;
            var seconds = time % 60;
            var sb = new StringBuilder();
            if (minutes > 0)
                sb.Append(minutes).Append(" м ");

            if (seconds > 0)
                sb.Append(seconds).Append(" с");

            return sb.ToString();
        }
    }
}