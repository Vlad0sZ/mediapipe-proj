using JetBrains.Annotations;
using Runtime.UI.Interfaces;
using Runtime.UI.Screen;

namespace Runtime.Machine.States
{
    [UsedImplicitly]
    public sealed class MainMenuState : IState
    {
        private readonly ICanvas _canvas;

        public MainMenuState(ICanvas canvas) =>
            _canvas = canvas;

        public void Activate()
        {
            var screen = _canvas.GetScreen(ScreenNames.MainMenu);
            screen?.Show();
        }

        public void Deactivate()
        {
            var screen = _canvas.GetScreen(ScreenNames.MainMenu);
            screen?.Hide();
        }
    }
}