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
        private readonly ILevelController _levelController;
        private readonly IPauseController _pauseController;
        private readonly ICanvas _canvas;
        private IDisposable _disposable;

        public GameState(ICanvas canvas, ILevelController levelController, IPauseController pauseController) : base(
            canvas, ScreenNames.Game)
        {
            _canvas = canvas;
            _levelController = levelController;
            _pauseController = pauseController;
        }

        public override async UniTask ActivateAsync(CancellationToken ct)
        {
            await base.ActivateAsync(ct);
            _levelController.StartLevel();
            _pauseController.StartControl();
            _disposable = _pauseController.OnPaused.Subscribe(OnPausedChanged);
        }

        public override async UniTask DeactivateAsync(CancellationToken ct)
        {
            _disposable?.Dispose();
            _pauseController.StopControl();
            _levelController.StopLevel();
            await base.DeactivateAsync(ct);
        }

        private void OnPausedChanged(bool isPaused)
        {
            if (isPaused)
            {
                _canvas.GetScreen(ScreenNames.Pause)?.Show();
                _levelController.Pause();
            }
            else
            {
                _canvas.GetScreen(ScreenNames.Pause)?.Hide();
                _levelController.Resume();
            }
        }
    }
}