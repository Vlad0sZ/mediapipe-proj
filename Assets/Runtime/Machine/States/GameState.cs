using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using Runtime.Game.Controllers;
using Runtime.Game.Interfaces;
using Runtime.UI.Interfaces;
using Runtime.UI.Screen;

namespace Runtime.Machine.States
{
    [UsedImplicitly]
    public sealed class GameState : UIState
    {
        private readonly IGameController _gameController;
        private readonly IPauseController _pauseController;
        private readonly ICanvas _canvas;
        private IDisposable _disposable;

        public GameState(ICanvas canvas, IGameController gameController, IPauseController pauseController) : base(
            canvas, ScreenNames.Game)
        {
            _canvas = canvas;
            _gameController = gameController;
            _pauseController = pauseController;
        }

        public override async UniTask ActivateAsync(CancellationToken ct)
        {
            await base.ActivateAsync(ct);
            _gameController.StartLevel();
            _pauseController.StartControl();
            _disposable = _pauseController.OnPaused.Subscribe(OnPausedChanged);
        }

        public override async UniTask DeactivateAsync(CancellationToken ct)
        {
            _disposable?.Dispose();
            _pauseController.StopControl();
            _gameController.StopLevel();
            await base.DeactivateAsync(ct);
        }

        private void OnPausedChanged(bool isPaused)
        {
            if (isPaused)
            {
                _canvas.GetScreen(ScreenNames.Pause)?.Show();
                _gameController.Pause();
            }
            else
            {
                _canvas.GetScreen(ScreenNames.Pause)?.Hide();
                _gameController.Resume();
            }
        }
    }
}