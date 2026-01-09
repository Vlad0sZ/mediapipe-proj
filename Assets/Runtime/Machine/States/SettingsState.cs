using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Runtime.Game.Embient;
using Runtime.UI.Interfaces;
using Runtime.UI.Screen;

namespace Runtime.Machine.States
{
    [UsedImplicitly]
    public sealed class SettingsState : UIState
    {
        private readonly ICameraController _cameraController;

        public SettingsState(ICanvas canvas, ICameraController cameraController) : base(canvas, ScreenNames.Settings)
        {
            _cameraController = cameraController;
        }

        public override UniTask ActivateAsync(CancellationToken ct)
        {
            _cameraController.LiveLevelCamera(1);
            return base.ActivateAsync(ct);
        }

        public override UniTask DeactivateAsync(CancellationToken ct)
        {
            _cameraController.LiveMainCamera();
            return base.DeactivateAsync(ct);
        }
    }
}