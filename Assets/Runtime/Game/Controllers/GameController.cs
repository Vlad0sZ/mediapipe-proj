using System;
using R3;
using Runtime.Game.Embient;
using Runtime.Game.Factories;
using Runtime.Game.Interfaces;
using Runtime.Game.Publishers;
using Runtime.Game.ScriptableData;
using Runtime.Game.Timers;
using Runtime.Game.UI;
using Runtime.Machine;
using Runtime.Machine.States;
using UnityEngine;
using VContainer;

namespace Runtime.Game
{
    public sealed class GameController : MonoBehaviour, IGameController, ILevelPublisher
    {
        [SerializeField] private int level;
        [SerializeField] private FoodObjects objectsData;
        [SerializeField] private GameSettings settingsData;
        [SerializeField] private FoodFactory factory;
        [SerializeField] private PrepareTaskUI taskUI;

        private IStateMachine _stateMachine;
        private ICameraController _cameraController;
        private IObjectSpawner _objectSpawner;
        private ITimer _timer;
        private IPlayerFactory _playerFactory;
        private IDisposable _timerSubscription;

        private readonly Subject<GameSettings.Settings> _settingsSubject = new();

        public Observable<GameSettings.Settings> SettingsChanged => _settingsSubject;


        [Inject]
        public void Construct(IObjectSpawner objectSpawner, ITimer timer, IPlayerFactory playerFactory,
            IStateMachine stateMachine, ICameraController cameraController)
        {
            _timer = timer;
            _stateMachine = stateMachine;
            _objectSpawner = objectSpawner;
            _playerFactory = playerFactory;
            _cameraController = cameraController;
        }


        public void SetupLevel()
        {
            var settings = settingsData.GetSettings(level);
            var objects = objectsData.GetNextGroup();

            taskUI.Setup(objects);
            factory.Setup(objects);
            _settingsSubject.OnNext(settings);
            _objectSpawner.Setup(settings.SpawnSettings);
            _timer.Setup(settings.LevelSettings);
        }

        public void StartLevel()
        {
            _timerSubscription = _timer.Event.Subscribe(OnTimerOver);

            _cameraController.LiveLevelCamera(level);
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
            _cameraController.LiveMainCamera();
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