using JetBrains.Annotations;
using Runtime.UI.Interfaces;
using Runtime.UI.Screen;

namespace Runtime.Machine.States
{
    [UsedImplicitly]
    public sealed class GameOverState : UIState
    {
        public GameOverState(ICanvas canvas) : base(canvas, ScreenNames.GameOver)
        {
        }
    }
}