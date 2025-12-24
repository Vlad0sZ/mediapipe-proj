using System;
using R3;
using Runtime.Game.Publishers;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Runtime.Game.UI
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private Slider scoreSlider;

        private IScorePublisher _scorePublisher;
        private IDisposable _disposable;

        [Inject]
        public void Construct(IScorePublisher scorePublisher) =>
            _scorePublisher = scorePublisher;

        private void OnEnable() =>
            _disposable = _scorePublisher.OnScore.Subscribe(UpdateScore);

        private void OnDisable() =>
            _disposable?.Dispose();

        public void SetScore() =>
            UpdateScore(_scorePublisher.Score);

        private void UpdateScore(ScoreModel model)
        {
            var score = model.Score;
            scoreSlider.value = score / 100f;
        }
    }
}