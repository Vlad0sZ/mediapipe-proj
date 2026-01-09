using System;
using R3;
using Runtime.Game.Embient;
using Runtime.Game.Factories;
using Runtime.Game.Interfaces;
using Runtime.Game.Timers;
using Runtime.Machine;
using Runtime.Machine.States;
using UnityEngine;
using VContainer;

namespace Runtime.Game.Controllers
{
    public sealed class GameController : MonoBehaviour, IGameController
    {
        private IStateMachine _stateMachine;
        private ICameraController _cameraController;
        private IObjectSpawner _objectSpawner;
        private ITimer _timer;
        private IPlayerFactory _playerFactory;
        private IDisposable _timerSubscription;

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

        public void StartLevel()
        {
            _timerSubscription = _timer.Event.Subscribe(OnTimerOver);

            _cameraController.LiveLevelCamera(0);
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
            _cameraController.LiveMainCamera();
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