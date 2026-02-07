using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Runtime.CharacterPersonalization
{
    public sealed class ClothingChangeComponent : MonoBehaviour
    {
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private string category;

        private CharacterCustomizer _customizer;

        [Inject]
        public void Construct(CharacterCustomizer customizer) => _customizer = customizer;

        private void OnEnable()
        {
            leftButton.onClick.AddListener(Prev);
            rightButton.onClick.AddListener(Next);
        }

        private void OnDisable()
        {
            leftButton.onClick.RemoveListener(Prev);
            rightButton.onClick.RemoveListener(Next);
        }

        private void Prev() =>
            _customizer.PreviousItem(category);

        private void Next() =>
            _customizer.NextItem(category);
    }
}