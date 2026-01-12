using System;
using R3;
using Runtime.Game.Publishers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Runtime.Game.UI
{
    public class EndlessScoreUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text scorePointsText;
        [SerializeField] private Image faceImage;
        [SerializeField] private Sprite sadFace;

        private IScorePublisher _scorePublisher;
        private IDisposable _disposable;

        [Inject]
        public void Construct(IScorePublisher scorePublisher) =>
            _scorePublisher = scorePublisher;

        private void OnEnable()
        {
            _disposable = _scorePublisher.OnScore.Subscribe(UpdateScore);
            UpdateScore(_scorePublisher.Score);
        }

        private void OnDisable() =>
            _disposable?.Dispose();

        private void UpdateScore(ScoreModel model)
        {
            var score = model.Progress();
            faceImage.overrideSprite = score >= 0 ? null : sadFace;
            scorePointsText.text = model.PositiveScore.ToString();
        }
    }
}