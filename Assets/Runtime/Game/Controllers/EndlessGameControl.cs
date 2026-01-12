using System;
using JetBrains.Annotations;
using R3;
using Runtime.Game.Interfaces;
using Runtime.Game.Publishers;
using Runtime.Game.ScriptableData;
using Runtime.Game.Spawner;
using Runtime.Game.Timers;
using UnityEngine;

namespace Runtime.Game.Controllers
{
    [UsedImplicitly]
    public sealed class EndlessGameControl : IGameControl
    {
        private readonly IObjectSpawner _objectSpawner;
        private readonly ISpawnerSetup _spawnerSetup;
        private readonly IScorePublisher _scorePublisher;
        private readonly IFoodGroupProvider _foodGroupProvider;
        private readonly ITimer _timer;

        private readonly Subject<Unit> _endGameSub = new();

        private int _difficultyLevel;
        private ISpawnerChain _speedChain;
        private ISpawnerChain _foodChain;
        private GameSettings.SpawnSettings _spawnSettings;
        private GameSettings.ObjectsSettings _speedSettings;

        private IDisposable _scoreSub;
        private IDisposable _timerSub;

        public EndlessGameControl(IObjectSpawner objectSpawner, IScorePublisher scorePublisher, ITimer timer,
            ISpawnerSetup spawnerSetup, IFoodGroupProvider foodGroupProvider)
        {
            _objectSpawner = objectSpawner;
            _scorePublisher = scorePublisher;
            _timer = timer;
            _spawnerSetup = spawnerSetup;
            _foodGroupProvider = foodGroupProvider;
        }

        public Observable<Unit> EndGame => _endGameSub;

        public void OnStart(IGameModeSettings settings)
        {
            _difficultyLevel = 0;
            var levelSettings = settings.GetLevelSettings();

            _spawnSettings = levelSettings.SpawnSettings;
            _speedSettings = levelSettings.ObjectsSettings;

            _foodChain = new ObjectFoodSetupChain(_foodGroupProvider);
            _spawnerSetup.AddSpawnerChain(_foodChain);

            IncreaseLevel();
            _objectSpawner.StartSpawn();
            _timer.StartTimer(30f);
            _timerSub = _timer.Event.Subscribe(OnTimerOver);
            _scoreSub = _scorePublisher.OnScore.Subscribe(OnScoreEvent);
        }

        public void OnPaused()
        {
            _objectSpawner.StopSpawn();
            _timer.Pause();
        }

        public void OnResumed()
        {
            _objectSpawner.StartSpawn();
            _timer.Resume();
        }

        public void OnStopped()
        {
            _timerSub?.Dispose();
            _timerSub = null;

            _scoreSub?.Dispose();
            _scoreSub = null;

            _spawnerSetup.RemoveSpawnerChain(_speedChain);
            _spawnerSetup.RemoveSpawnerChain(_foodChain);

            _timer.StopTimer();
            _objectSpawner.StopSpawn();
        }


        private void OnTimerOver(ElapsedTime model)
        {
            if (model.Progress < 1f)
                return;

            IncreaseLevel();
            _timer.StartTimer(30f);
        }

        private void IncreaseLevel()
        {
            _difficultyLevel++;
            _objectSpawner.Configure(IncreaseDifficulty());

            if (_speedChain != null)
                _spawnerSetup.RemoveSpawnerChain(_speedChain);

            _speedChain = new ObjectSpeedSetupChain(IncreaseSpeedChain());
            _spawnerSetup.AddSpawnerChain(_speedChain);
        }

        private void OnScoreEvent(ScoreModel model)
        {
            var negative = model.NegativeScore.Abs();
            var positive = model.PositiveScore;
            var different = negative > positive ? negative - positive : 0;

            if (different > 100 && model.Progress() < -0.55f)
                _endGameSub.OnNext(default);
        }


        private GameSettings.ObjectsSettings IncreaseSpeedChain()
        {
            var minMaxFallSpeed = _speedSettings.minMaxFallSpeed;
            var rotationSpeed = _speedSettings.minMaxRotationSpeed;

            float fallSpeedIncrease = 1f - Mathf.Pow(0.9f, _difficultyLevel);
            float fallNextSpeed = 1f - Mathf.Pow(0.9f, _difficultyLevel + 1);

            float min = Mathf.Lerp(minMaxFallSpeed.x, minMaxFallSpeed.y, fallSpeedIncrease);
            float max = Mathf.Lerp(minMaxFallSpeed.x, minMaxFallSpeed.y, fallNextSpeed);

            var settings = new GameSettings.ObjectsSettings()
            {
                minMaxRotationSpeed = rotationSpeed,
                minMaxFallSpeed = new Vector2(min, max)
            };

            return settings;
        }

        private GameSettings.SpawnSettings IncreaseDifficulty()
        {
            int objectIncrease = _difficultyLevel / 2;
            int delayIncrease = _difficultyLevel / 3;

            int maxObjects = 1 + objectIncrease;
            float spawnDelay = 1.5f - (delayIncrease * 0.15f);

            GameSettings.SpawnSettings spawnSettings = default;
            spawnSettings.maxObjectPerSpawn = Mathf.Min(_spawnSettings.maxObjectPerSpawn, maxObjects);
            spawnSettings.spawnDelay = Mathf.Max(
                spawnDelay,
                _spawnSettings.spawnDelay
            );

            UnityEngine.Debug.Log(
                $"new delay = {spawnSettings.spawnDelay}, new objects = {spawnSettings.maxObjectPerSpawn}");
            return spawnSettings;
        }
    }
}