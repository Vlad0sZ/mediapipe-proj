using JetBrains.Annotations;
using Runtime.UI.Interfaces;
using Runtime.UI.Screen;

namespace Runtime.Machine.States
{
    [UsedImplicitly]
    public sealed class NoWebCamState : IState
    {
        private readonly ICanvas _canvas;

        public NoWebCamState(ICanvas canvas) => _canvas = canvas;

        public void Activate()
        {
            var screen = _canvas.GetScreen(ScreenNames.NoCamera);
            screen?.Show();
        }

        public void Deactivate()
        {
            var screen = _canvas.GetScreen(ScreenNames.NoCamera);
            screen?.Hide();
        }
    }
}