using JetBrains.Annotations;
using Runtime.UI.Interfaces;
using Runtime.UI.Screen;

namespace Runtime.Machine.States
{
    [UsedImplicitly]
    public class SettingsState : IState
    {
        private readonly ICanvas _canvas;

        public SettingsState(ICanvas canvas) =>
            _canvas = canvas;

        public void Activate()
        {
            var screen = _canvas.GetScreen(ScreenNames.Settings);
            screen?.Show();
        }

        public void Deactivate()
        {
            var screen = _canvas.GetScreen(ScreenNames.Settings);
            screen?.Hide();
        }
    }
}