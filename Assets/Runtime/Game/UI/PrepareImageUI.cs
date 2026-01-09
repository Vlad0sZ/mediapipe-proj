using System;
using System.Collections.Generic;
using R3;
using Runtime.Game.Interfaces;
using Runtime.Types;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VContainer;

namespace Runtime.Game.UI
{
    public sealed class PrepareImageUI : AbstractGameScreenUI
    {
        [SerializeField] private TMP_Text textComponent;
        [SerializeField] private float raiseTime;
        [SerializeField] private Image fillImage;
        [SerializeField] private AnimationCurve fillCurve;

        [SerializeField] private GameObject[] imageStates;
        [SerializeField] private UnityEvent onRaised;

        private const string HandRaisedText = "Для начала игры поднимите две руки.";

        private readonly Dictionary<HandRaiseType, string> _texts = new()
        {
            [HandRaiseType.None] = "Сначала нужно встать перед камерой",
            [HandRaiseType.LeftHandBelow | HandRaiseType.RightHandRaised] = "Поднимите левую руку",
            [HandRaiseType.LeftHandRaised | HandRaiseType.RightHandBelow] = "Поднимите правую руку",
            [HandRaiseType.HandsRaised] = "Удерживайте..."
        };

        private readonly Dictionary<HandRaiseType, int> _arrayIndexes = new()
        {
            [HandRaiseType.None] = 1,
            [HandRaiseType.LeftHandBelow | HandRaiseType.RightHandRaised] = 2,
            [HandRaiseType.LeftHandRaised | HandRaiseType.RightHandBelow] = 3,
        };

        private IPlayerRaisePublisher _raisePublisher;

        private IDisposable _disposable;
        private float _timeToRaise;

        [Inject]
        public void Construct(IPlayerRaisePublisher publisher)
        {
            _raisePublisher = publisher;
        }

        private void Start() =>
            fillImage.fillAmount = 0f;

        protected override void OnScreenShown()
        {
            fillImage.fillAmount = 0f;
            _timeToRaise = 0f;
            _disposable = _raisePublisher.PlayerEvent.Subscribe(OnPlayerPose);
        }

        protected override void OnScreenHidden() =>
            _disposable?.Dispose();


        private void OnPlayerPose(PlayerPose model)
        {
            UpdateTime(model.HandRaiseType);
            UpdateText(model.HandRaiseType);
            UpdateGameObjects(model.HandRaiseType);

            if (model.IsVisible == false)
                return;

            UpdateFillImage();

            if (_timeToRaise < raiseTime)
                return;

            onRaised.Invoke();
        }

        private void UpdateText(HandRaiseType handRaiseType)
        {
            string text = _texts.GetValueOrDefault(handRaiseType, HandRaisedText);
            textComponent.text = text;
        }

        private void UpdateTime(HandRaiseType handRaiseType)
        {
            float timeToRaise = _timeToRaise;
            _timeToRaise = handRaiseType == HandRaiseType.HandsRaised ? timeToRaise + Time.deltaTime : 0f;
        }

        private void UpdateGameObjects(HandRaiseType handRaiseType)
        {
            int activeIndex = _arrayIndexes.GetValueOrDefault(handRaiseType, 0);
            for (var i = 0; i < imageStates.Length; i++)
                imageStates[i].SetActive(i == activeIndex);
        }

        private void UpdateFillImage()
        {
            if (!fillImage) return;

            float progress = Mathf.Clamp01(_timeToRaise / raiseTime);
            fillImage.fillAmount = fillCurve.Evaluate(progress);
        }
    }
}