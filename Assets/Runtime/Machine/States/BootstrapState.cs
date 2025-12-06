using JetBrains.Annotations;
using Runtime.Infrastructure.Video;
using Runtime.UI.Interfaces;

namespace Runtime.Machine.States
{
    [UsedImplicitly]
    public sealed class BootstrapState : IState
    {
        private readonly IStateMachine _stateMachine;
        private readonly IWebCamInitializer _webCamInitializer;

        public BootstrapState(IStateMachine stateMachine, IWebCamInitializer webCamInitializer)
        {
            _stateMachine = stateMachine;
            _webCamInitializer = webCamInitializer;
        }

        public void Activate()
        {
            var isWebCamInitialized = _webCamInitializer.IsWebcamInitialized();
            if (isWebCamInitialized == false)
                _stateMachine.ChangeState<NoWebCamState>();

            _stateMachine.ChangeState<LoadingGameState>();
        }

        public void Deactivate()
        {
        }
    }
}