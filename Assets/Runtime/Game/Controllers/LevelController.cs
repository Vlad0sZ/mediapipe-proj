using System;
using JetBrains.Annotations;
using R3;
using Runtime.Game.Embient;
using Runtime.Game.Factories;
using Runtime.Game.Interfaces;
using Runtime.Machine;
using Runtime.Machine.States;
using UnityEngine;

namespace Runtime.Game.Controllers
{
    [UsedImplicitly]
    public sealed class LevelController : ILevelController
    {
        private readonly IStateMachine _stateMachine;
        private readonly ICameraController _cameraController;
        private readonly IPlayerFactory _playerFactory;
        private readonly IGameControlFactory _gameControlFactory;
        private readonly IGameModeSettings _gameModeSettings;

        private IGameControl _createdControl;
        private IDisposable _controlSub;

        public LevelController(IStateMachine stateMachine, ICameraController cameraController,
            IPlayerFactory playerFactory, IGameControlFactory gameControlFactory, IGameModeSettings gameModeSettings)
        {
            _stateMachine = stateMachine;
            _cameraController = cameraController;
            _playerFactory = playerFactory;
            _gameControlFactory = gameControlFactory;
            _gameModeSettings = gameModeSettings;
        }


        public void StartLevel()
        {
            var mode = _gameModeSettings.CurrentMode;
            _createdControl = _gameControlFactory.GenerateGameControl(mode);
            _playerFactory.SpawnPlayer(Vector3.zero);
            _cameraController.LiveLevelCamera(0);
            _createdControl.OnStart(_gameModeSettings);
            _controlSub = _createdControl.EndGame.Subscribe(EndLevel);
        }

        public void Pause() =>
            _createdControl?.OnPaused();

        public void Resume() =>
            _createdControl?.OnResumed();

        public void StopLevel()
        {
            _createdControl?.OnStopped();
            _controlSub?.Dispose();

            _controlSub = null;
            _createdControl = null;

            _cameraController.LiveMainCamera();
            _playerFactory.RemovePlayer();
            _playerFactory.SetupPlayer(null);
        }


        private void EndLevel(Unit unit) =>
            _stateMachine.ChangeState<GameOverState>();
    }
}