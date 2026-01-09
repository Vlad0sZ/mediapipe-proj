using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.UI.Interfaces;

namespace Runtime.Machine.States
{
    public abstract class UIState : IAsyncState
    {
        private readonly ScreenActivator _screenActivator;

        protected UIState(ICanvas canvas, string screenName) =>
            _screenActivator = new ScreenActivator(screenName, canvas);

        public virtual async UniTask ActivateAsync(CancellationToken ct) =>
            await _screenActivator.ChangeStateAsync(true, ct);

        public virtual async UniTask DeactivateAsync(CancellationToken ct) =>
            await _screenActivator.ChangeStateAsync(false, ct);
    }
}