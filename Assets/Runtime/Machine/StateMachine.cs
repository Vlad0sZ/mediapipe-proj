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
            ChangeStateAsync(state, _cts.Token).Forget();
        }

        private async UniTask ChangeStateAsync(IAsyncState state, CancellationToken ct)
        {
            try
            {
                await DeactivateStateAsync(_currentState, ct);
                _currentState = state;
                await ActivateStateAsync(_currentState, ct);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                UnityEngine.Debug.Log("exceptions when state changed:");
                UnityEngine.Debug.LogError(ex);
            }
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