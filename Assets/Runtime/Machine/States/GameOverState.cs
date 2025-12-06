using Runtime.UI.Interfaces;
using Runtime.UI.Screen;

namespace Runtime.Machine.States
{
    public sealed class GameOverState : IState
    {
        private readonly ICanvas _canvas;

        public GameOverState(ICanvas canvas) => _canvas = canvas;

        public void Activate() => _canvas.GetScreen(ScreenNames.GameOver)?.Show();

        public void Deactivate() => _canvas.GetScreen(ScreenNames.GameOver)?.Hide();
    }
}