using JetBrains.Annotations;
using Runtime.Machine;
using Runtime.Machine.States;
using VContainer.Unity;

namespace Runtime.Scope
{
    [UsedImplicitly]
    public class BootstrapEntryPoint : IStartable
    {
        private readonly IStateMachine _stateMachine;

        public BootstrapEntryPoint(IStateMachine stateMachine) =>
            _stateMachine = stateMachine;


        public void Start() => _stateMachine.ChangeState<BootstrapState>();
    }
}