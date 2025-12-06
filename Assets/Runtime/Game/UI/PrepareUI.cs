using System;
using R3;
using Runtime.Game.Interfaces;
using Runtime.Machine;
using Runtime.Machine.States;
using Runtime.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Runtime.Game.UI
{
    public class PrepareUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text textComponent;
        [SerializeField] private float raiseTime;
        [SerializeField] private Image fillImage;
        [SerializeField] private AnimationCurve fillCurve;

        private IPlayerRaisePublisher _raisePublisher;
        private IStateMachine _stateMachine;

        private IDisposable _disposable;
        private float _timeToRaise;

        [Inject]
        public void Construct(IPlayerRaisePublisher publisher, IStateMachine stateMachine)
        {
            _raisePublisher = publisher;
            _stateMachine = stateMachine;
        }

        private void Start() =>
            fillImage.fillAmount = 0f;

        private void OnEnable() =>
            _disposable = _raisePublisher.PlayerEvent.Subscribe(OnPlayerPose);

        private void OnDisable() =>
            _disposable?.Dispose();


        private void OnPlayerPose(PlayerPose model)
        {
            if (model.IsVisible == false)
            {
                textComponent.text = "Сначала нужно встать перед камерой";
                _timeToRaise = 0f;
                return;
            }

            switch (model.HandRaiseType)
            {
                case HandRaiseType.LeftHandBelow | HandRaiseType.RightHandRaised:
                    textComponent.text = "Поднимите левую руку";
                    _timeToRaise = 0f;
                    break;

                case HandRaiseType.LeftHandRaised | HandRaiseType.RightHandBelow:
                    textComponent.text = "Поднимите правую руку";
                    _timeToRaise = 0f;
                    break;

                case HandRaiseType.HandsRaised:
                    textComponent.text = "Удерживайте...";
                    _timeToRaise += Time.deltaTime;
                    break;


                default:
                    textComponent.text = "Для начала игры поднимите две руки.";
                    _timeToRaise = 0f;
                    break;
            }

            if (fillImage)
            {
                var progress = Mathf.Clamp01(_timeToRaise / raiseTime);
                fillImage.fillAmount = fillCurve.Evaluate(progress);
            }

            if (_timeToRaise >= raiseTime)
            {
                this.enabled = false;
                UnityEngine.Debug.Log("Ура начало игры");
                _stateMachine.ChangeState<GameState>();
            }
        }
    }
}