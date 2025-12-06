using System;

namespace Runtime.Machine
{
    public class StateMachine : IStateMachine
    {
        private readonly IStateFactory _stateFactory;
        private IState _currentState;

        public StateMachine(IStateFactory stateFactory) =>
            _stateFactory = stateFactory;

        public void ChangeState<T>() where T : IState
        {
            var state = _stateFactory.CreateState<T>();
            if (state == null)
                throw new Exception($"Can not find state by type {typeof(T)}");

            _currentState?.Deactivate();
            _currentState = state;
            _currentState.Activate();
        }
    }
}