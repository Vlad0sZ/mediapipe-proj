using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.UI.Interfaces;
using Runtime.UI.Screen;

namespace Runtime.Machine.States
{
    internal sealed class ScreenActivator
    {
        private readonly string _screenName;
        private readonly ICanvas _canvas;

        public ScreenActivator(string screenName, ICanvas canvas)
        {
            _screenName = screenName;
            _canvas = canvas;
        }

        public async UniTask ChangeStateAsync(bool isVisible, CancellationToken ct)
        {
            var screen = _canvas.GetScreen(_screenName);
            if (screen == null)
                return;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var completionSource = new UniTaskCompletionSource<bool>();

            cts.Token.Register(() => completionSource.TrySetCanceled(ct));

            if (isVisible)
                screen.Show(callback: () => completionSource.TrySetResult(true));
            else
                screen.Hide(callback: () => completionSource.TrySetResult(true));

            await completionSource.Task.AttachExternalCancellation(ct);
        }
    }
}