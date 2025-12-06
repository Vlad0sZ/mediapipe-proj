using System;
using R3;
using Runtime.Game.Factories;
using Runtime.Game.Interfaces;
using Runtime.Game.ScriptableData;
using Runtime.Game.Timers;
using Runtime.Game.UI;
using Runtime.Machine;
using Runtime.Machine.States;
using UnityEngine;
using VContainer;

namespace Runtime.Game
{
    public sealed class GameController : MonoBehaviour, IGameController
    {
        [SerializeField] private int level;
        [SerializeField] private FoodObjects objectsData;
        [SerializeField] private GameSettings settingsData;
        [SerializeField] private FoodFactory factory;
        [SerializeField] private PrepareTaskUI taskUI;

        private IStateMachine _stateMachine;
        private IObjectSpawner _objectSpawner;
        private ITimer _timer;
        private IPlayerFactory _playerFactory;
        private IDisposable _timerSubscription;

        [Inject]
        public void Construct(IObjectSpawner objectSpawner, ITimer timer, IPlayerFactory playerFactory,
            IStateMachine stateMachine)
        {
            _timer = timer;
            _stateMachine = stateMachine;
            _objectSpawner = objectSpawner;
            _playerFactory = playerFactory;
        }

        public void SetupLevel()
        {
            var timerSettings = settingsData.GetLevelSettings(level);
            var settings = settingsData.GetSpawnSettings(level);
            var objects = objectsData.GetNextGroup();

            taskUI.Setup(objects);
            factory.Setup(objects);
            _objectSpawner.Setup(settings);
            _timer.Setup(timerSettings);
        }

        public void StartLevel()
        {
            _timerSubscription = _timer.Event.Subscribe(OnTimerOver);

            _timer.StartTimer();
            _objectSpawner.StartSpawn();
            _playerFactory.SpawnPlayer(Vector3.zero);
        }

        public void Pause()
        {
            _objectSpawner.StopSpawn();
            _timer.Pause();
        }

        public void Resume()
        {
            _objectSpawner.StartSpawn();
            _timer.Resume();
        }

        public void StopLevel()
        {
            _timerSubscription?.Dispose();
            _objectSpawner.StopSpawn();
            _timer.StopTimer();
            _playerFactory.RemovePlayer();
        }

        private void OnTimerOver(ElapsedTime result)
        {
            if (result.Progress < 1f)
                return;

            _stateMachine.ChangeState<GameOverState>();
        }
    }
}