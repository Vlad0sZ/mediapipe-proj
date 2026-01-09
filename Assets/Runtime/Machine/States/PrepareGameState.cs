using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Runtime.Game.Embient;
using Runtime.Game.Interfaces;
using Runtime.UI.Interfaces;
using Runtime.UI.Screen;

namespace Runtime.Machine.States
{
    [UsedImplicitly]
    public sealed class PrepareGameState : UIState
    {
        private readonly ILevelSetup _levelSetup;

        private readonly ICameraController _cameraController;

        public PrepareGameState(ICanvas canvas, ILevelSetup levelSetup, ICameraController cameraController) : base(
            canvas, ScreenNames.GamePrepare)
        {
            _levelSetup = levelSetup;
            _cameraController = cameraController;
        }

        // TODO avatar controller  + hands up.
        // TODO generate task here.

        public override async UniTask ActivateAsync(CancellationToken ct)
        {
            _cameraController.LiveLevelCamera(2);
            _levelSetup.SetupLevel();
            await base.ActivateAsync(ct);
        }
    }
}