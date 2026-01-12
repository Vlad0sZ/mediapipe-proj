using System.Text;
using Runtime.Game.Interfaces;
using Runtime.Game.Models;
using Runtime.Game.Types;
using Runtime.UI;
using TMPro;
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
        [SerializeField] private TMP_InputField nameField;
        [SerializeField] private Button playButton;

        private IGameModeSettings _gameSettings;
        private bool _validateRecord;

        [Inject]
        public void Construct(IGameModeSettings gameSettings) =>
            _gameSettings = gameSettings;

        private void Start() => subText.Formatter = this;

        protected override void OnScreenShowing()
        {
            _gameSettings.SetupGameMode(GameMode.Classic);
            _gameSettings.SetLevelTime(0.5f);
            gameModeGroup.SetToggleOn(1, true);
            nameField.text = _gameSettings.GetPlayerName();
            timeSlider.value = 0.5f;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            gameModeGroup.OnValueChanged.AddListener(OnModeChanged);
            timeSlider.onValueChanged.AddListener(OnSliderChanged);
            nameField.onValueChanged.AddListener(OnInputChanged);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            gameModeGroup.OnValueChanged.RemoveListener(OnModeChanged);
            timeSlider.onValueChanged.RemoveListener(OnSliderChanged);
            nameField.onValueChanged.RemoveListener(OnInputChanged);
        }

        private void OnModeChanged(int index)
        {
            var gm = (GameMode) index;
            var time = gm == GameMode.Classic ? timeSlider.value : 0f;
            _validateRecord = gm == GameMode.Endless;

            _gameSettings.SetupGameMode(gm);
            _gameSettings.SetLevelTime(time);
            OnInputChanged(nameField.text);
        }

        private void OnInputChanged(string text)
        {
            bool isTextExists = !string.IsNullOrWhiteSpace(text);
            playButton.interactable = !_validateRecord || isTextExists;
            _gameSettings.SetPlayerName(text);
        }

        private void OnSliderChanged(float value) =>
            _gameSettings.SetLevelTime(value);

        public string Format(float clampedValue)
        {
            var levelTime = _gameSettings?.GetLevelSettings()?.LevelSettings ?? default;
            if (levelTime.Endless())
                return string.Empty;

            float timeClamp = Mathf.Lerp(levelTime.minLevelTime, levelTime.maxLevelTime, clampedValue);
            var time = new TimeModel(timeClamp);

            var sb = new StringBuilder();
            if (time.Minutes > 0)
                sb.Append(time.Minutes).Append(" м ");

            if (time.Seconds > 0)
                sb.Append(time.Seconds).Append(" с");

            return sb.ToString();
        }
    }
}