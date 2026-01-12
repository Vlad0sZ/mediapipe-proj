using System.Collections.Generic;
using Runtime.Game.Interfaces;
using Runtime.Game.Models;
using Runtime.Game.Publishers;
using Runtime.Game.ScriptableData;
using Runtime.Game.Types;
using UnityEngine;
using VContainer;

namespace Runtime.Game.Controllers
{
    public sealed class GameModeSettingsController : MonoBehaviour,
        IGameModeSettings,
        ILevelSetup
    {
        [SerializeField] private GameSettings gameSettings;
        [SerializeField] private FoodObjects foodObjects;

        private GameSettings.Settings _settings;
        private readonly List<IGameModePayload> _modePayloads = new List<IGameModePayload>(16);
        private readonly List<IFoodPayload> _foodPayloads = new List<IFoodPayload>(16);

        private float _levelTime;
        private GameMode _currentMode;
        private string _playerName;

        [Inject]
        public void Construct(IEnumerable<IGameModePayload> modsPayloads,
            IEnumerable<IFoodPayload> foodPayloads)
        {
            _modePayloads.AddRange(modsPayloads);
            _foodPayloads.AddRange(foodPayloads);
        }

        public void SetupGameMode(GameMode gameMode) =>
            CurrentMode = gameMode;

        public void SetPlayerName(string playerName) =>
            _playerName = playerName;

        public string GetPlayerName() =>
            _playerName;

        public void SetLevelTime(float clamp01) =>
            _levelTime = Mathf.Clamp01(clamp01);

        public TimeModel GetLevelTime()
        {
            var settings = GetLevelSettings().LevelSettings;
            if (settings.Endless())
                return default;

            var seconds = Mathf.Lerp(settings.minLevelTime, settings.maxLevelTime, _levelTime);
            return new TimeModel(seconds);
        }

        public GameSettings.Settings GetLevelSettings() =>
            _settings;

        public GameMode CurrentMode
        {
            get => _currentMode;
            private set
            {
                _currentMode = value;
                _settings = GenerateSettingsByMode(_currentMode);
            }
        }

        public void SetupLevel()
        {
            if (_settings == null)
                CurrentMode = GameMode.Classic;

            var objectsData = CurrentMode == GameMode.Training
                ? foodObjects.GetTrainingGroup()
                : foodObjects.GetNextGroup();

            foreach (var payload in _modePayloads)
                payload.Setup(this);

            foreach (var payload in _foodPayloads)
                payload.Setup(objectsData);
        }

        private GameSettings.Settings GenerateSettingsByMode(GameMode mode) =>
            gameSettings.GetSettings((int) mode);
    }
}