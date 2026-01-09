using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Runtime.Game.Embient;
using Runtime.UI.Interfaces;
using Runtime.UI.Screen;

namespace Runtime.Machine.States
{
    [UsedImplicitly]
    public sealed class GameModeState : UIState
    {

        public GameModeState(ICanvas canvas, ICameraController cameraController) : base(canvas, ScreenNames.GameMode)
        {
        }

    }
}