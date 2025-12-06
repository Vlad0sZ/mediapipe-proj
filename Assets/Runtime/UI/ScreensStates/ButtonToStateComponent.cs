using Runtime.Machine;
using Runtime.Machine.States;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;

namespace Runtime.UI
{
    public abstract class ButtonToStateComponent<T> : MonoBehaviour
        where T : IState
    {
        [SerializeField] private Button button;

        private IStateMachine _stateMachine;

        [Inject]
        public void Construct(IStateMachine stateMachine) =>
            _stateMachine = stateMachine;

        private void OnEnable() =>
            button.onClick.AddListener(StartClicked);

        private void OnDisable() =>
            button.onClick.RemoveListener(StartClicked);

        private void StartClicked() =>
            _stateMachine?.ChangeState<T>();
    }
}