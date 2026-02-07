using Runtime.Game.Factories;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Runtime.CharacterPersonalization
{
    public sealed class CustomizationScreen : MonoBehaviour
    {
        [SerializeField] private Button resetButton;
        [SerializeField] private Button applyButton;
        private CharacterCustomizer _customizer;
        private IPlayerFactory _playerFactory;

        [Inject]
        public void Construct(CharacterCustomizer customizer, IPlayerFactory playerFactory)
        {
            _customizer = customizer;
            _playerFactory = playerFactory;
        }

        private void OnEnable()
        {
            resetButton.onClick.AddListener(ResetCharacter);
            applyButton.onClick.AddListener(ApplyCharacter);
        }

        private void OnDisable()
        {
            resetButton.onClick.RemoveListener(ResetCharacter);
            applyButton.onClick.RemoveListener(ApplyCharacter);
        }

        private void ApplyCharacter()
        {
            var skin = _customizer.GetCharacterCustomization();
            _playerFactory.SetupPlayer(skin);
        }

        private void ResetCharacter() =>
            _customizer.ResetItems();
    }
}