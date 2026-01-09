using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Runtime.Infrastructure.Video;

namespace Runtime.Machine.States
{
    [UsedImplicitly]
    public sealed class BootstrapState : IAsyncState
    {
        private readonly IStateMachine _stateMachine;
        private readonly IWebCamInitializer _webCamInitializer;

        public BootstrapState(IStateMachine stateMachine, IWebCamInitializer webCamInitializer)
        {
            _stateMachine = stateMachine;
            _webCamInitializer = webCamInitializer;
        }

        public UniTask ActivateAsync(CancellationToken ct)
        {
            var isWebCamInitialized = _webCamInitializer.IsWebcamInitialized();
            if (isWebCamInitialized == false)
                _stateMachine.ChangeState<NoWebCamState>();

            _stateMachine.ChangeState<LoadingGameState>();
            return UniTask.CompletedTask;
        }

        public UniTask DeactivateAsync(CancellationToken ct) =>
            UniTask.CompletedTask;
    }
}