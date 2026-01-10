using System.Collections.Generic;
using Runtime.Game.Interfaces;
using Runtime.Game.Publishers;
using Runtime.Game.ScriptableData;
using Runtime.Game.Types;
using UnityEngine;
using VContainer;

namespace Runtime.Game.Controllers
{
    public sealed class GameModeSettingsController : MonoBehaviour, IGameModeSettings, ILevelSetup
    {
        [SerializeField] private GameSettings gameSettings;
        [SerializeField] private FoodObjects foodObjects;

        private GameSettings.Settings _settings;
        private ILevelPublisherSetup _levelPublisher;
        private readonly List<IGameModePayload> _modePayloads = new List<IGameModePayload>(16);
        private readonly List<IFoodPayload> _foodPayloads = new List<IFoodPayload>(16);

        private float _levelTime;
        private GameMode _currentMode;

        [Inject]
        public void Construct(IEnumerable<IGameModePayload> modsPayloads,
            IEnumerable<IFoodPayload> foodPayloads,
            ILevelPublisherSetup levelPublisher)
        {
            _levelPublisher = levelPublisher;
            _modePayloads.AddRange(modsPayloads);
            _foodPayloads.AddRange(foodPayloads);
        }

        public void SetupGameMode(GameMode gameMode) =>
            CurrentMode = gameMode;

        public void SetLevelTime(float clamp01) =>
            _levelTime = Mathf.Clamp01(clamp01);

        public float GetLevelTime()
        {
            var settings = GetLevelTimeSettings();
            if (settings.Endless())
                return 0;

            return Mathf.Lerp(settings.minLevelTime, settings.maxLevelTime, _levelTime);
        }

        public GameSettings.LevelSettings GetLevelTimeSettings()
        {
            var time = _settings?.LevelSettings ?? default;
            return time;
        }

        public GameMode CurrentMode
        {
            get => _currentMode;
            private set
            {
                _currentMode = value;
                _settings = GenerateSettingsByMode(_currentMode);
                _levelPublisher.Publish(_settings);
            }
        }

        public void SetupLevel()
        {
            if (_settings == null)
                CurrentMode = GameMode.Classic;

            var objectsData = foodObjects.GetNextGroup();

            foreach (var payload in _modePayloads)
                payload.Setup(this);

            foreach (var payload in _foodPayloads)
                payload.Setup(objectsData);
        }

        private GameSettings.Settings GenerateSettingsByMode(GameMode mode) =>
            gameSettings.GetSettings((int) mode);
    }
}