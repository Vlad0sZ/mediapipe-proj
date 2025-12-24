using JetBrains.Annotations;
using Runtime.Game.Interfaces;
using Runtime.UI.Interfaces;
using Runtime.UI.Screen;

namespace Runtime.Machine.States
{
    [UsedImplicitly]
    public sealed class GameState : IState
    {
        private readonly IGameController _gameController;
        private readonly ICanvas _canvas;

        public GameState(ICanvas canvas, IGameController gameController)
        {
            _canvas = canvas;
            _gameController = gameController;
        }

        public void Activate()
        {
            _canvas.GetScreen(ScreenNames.Game)?.Show();
            _gameController.StartLevel();
        }

        public void Deactivate()
        {
            _canvas.GetScreen(ScreenNames.Game)?.Hide();
            _gameController.StopLevel();
        }
    }
}