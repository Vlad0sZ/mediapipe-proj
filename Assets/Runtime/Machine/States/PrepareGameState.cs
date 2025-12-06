using JetBrains.Annotations;
using Runtime.Game.Interfaces;
using Runtime.UI.Interfaces;
using Runtime.UI.Screen;

namespace Runtime.Machine.States
{
    [UsedImplicitly]
    public sealed class PrepareGameState : IState
    {
        private readonly ICanvas _canvas;
        private readonly IGameController _gameController;

        public PrepareGameState(ICanvas canvas, IGameController gameController)
        {
            _canvas = canvas;
            _gameController = gameController;
        }

        // TODO avatar controller  + hands up.
        // TODO generate task here.

        public void Activate()
        {
            _gameController.SetupLevel();
            _canvas.GetScreen(ScreenNames.GamePrepare)?.Show();
        }

        public void Deactivate() =>
            _canvas.GetScreen(ScreenNames.GamePrepare)?.Hide();
    }
}