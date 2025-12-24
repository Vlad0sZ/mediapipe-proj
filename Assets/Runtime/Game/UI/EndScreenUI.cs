using Runtime.Game.Publishers;
using TMPro;
using UnityEngine;
using VContainer;

namespace Runtime.Game.UI
{
    public class EndScreenUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text textComponent;

        private IScorePublisher _scorePublisher;

        [Inject]
        public void Construct(IScorePublisher scorePublisher)
        {
            _scorePublisher = scorePublisher;
        }

        public void OnScreenChanged(bool isVisible)
        {
            UnityEngine.Debug.Log($"change {isVisible} to gameover");
            if (isVisible)
                UpdateText();
        }

        private void UpdateText()
        {
            var score = _scorePublisher.Score.Score;
            textComponent.text = score switch
            {
                > 0 => "Ура, ужин состоялся!",
                < 0 => "Кажется, ужин пошёл не по плану.",
                _ => "Что-то ни туда, ни сюда..."
            };
        }
    }
}