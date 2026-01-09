using System;
using Runtime.Machine;
using UnityEngine;
using VContainer;

namespace Runtime.UI
{
    public sealed class StateChangeComponent : MonoBehaviour
    {
        [SelectType(baseType: typeof(IAsyncState))] [SerializeField]
        private string state;

        private IStateMachine _stateMachine;

        [Inject]
        public void Construct(IStateMachine stateMachine) =>
            _stateMachine = stateMachine;

        public void ChangeState()
        {
            Type stateType = string.IsNullOrEmpty(state) ? null : Type.GetType(state);
            if (stateType == null) UnityEngine.Debug.LogWarning($"state inside {gameObject} is null.");
            else _stateMachine.ChangeState(stateType);
        }
    }
}