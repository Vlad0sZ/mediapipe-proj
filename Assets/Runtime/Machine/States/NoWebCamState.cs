using JetBrains.Annotations;
using Runtime.UI.Interfaces;
using Runtime.UI.Screen;

namespace Runtime.Machine.States
{
    [UsedImplicitly]
    public sealed class NoWebCamState : UIState
    {
        public NoWebCamState(ICanvas canvas) : base(canvas, ScreenNames.NoCamera)
        {
        }
    }
}