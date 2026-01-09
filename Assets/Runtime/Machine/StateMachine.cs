using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Runtime.Machine
{
    [UsedImplicitly]
    public sealed class StateMachine : IStateMachine
    {
        private readonly IStateFactory _stateFactory;
        private CancellationTokenSource _cts;
        private IAsyncState _currentState;

        public StateMachine(IStateFactory stateFactory) =>
            _stateFactory = stateFactory;

        public void ChangeState<T>() where T : IAsyncState =>
            ChangeState(typeof(T));

        public void ChangeState(Type type)
        {
            var state = _stateFactory.CreateState(type);
            if (state == null)
                throw new Exception($"Can not find state by type {type}");

            _cts?.Cancel();
            _cts?.Dispose();

            _cts = new CancellationTokenSource();
            _ = ChangeStateAsync(state, _cts.Token);
        }

        private async UniTask ChangeStateAsync(IAsyncState state, CancellationToken ct)
        {
            await DeactivateStateAsync(_currentState, ct);
            _currentState = state;
            await ActivateStateAsync(_currentState, ct);
        }

        private static async UniTask DeactivateStateAsync(IAsyncState state, CancellationToken ct)
        {
            if (state == null || ct.IsCancellationRequested)
                return;

            await state.DeactivateAsync(ct);
        }

        private static async UniTask ActivateStateAsync(IAsyncState state, CancellationToken ct)
        {
            if (state == null || ct.IsCancellationRequested)
                return;

            await state.ActivateAsync(ct);
        }
    }
}