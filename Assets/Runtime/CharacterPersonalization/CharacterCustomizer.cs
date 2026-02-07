using System.Collections.Generic;
using UnityEngine;

namespace Runtime.CharacterPersonalization
{
    public class CharacterCustomizer : MonoBehaviour
    {
        public List<ClothingCategory> categories;

        // Словарь для хранения текущего индекса каждой категории
        private Dictionary<string, int> _currentIndices = new Dictionary<string, int>();

        private void Awake()
        {
            // Инициализируем индексы нулями
            foreach (var cat in categories)
            {
                _currentIndices[cat.categoryName] = 0;
                UpdateAppearance(cat.categoryName);
            }
        }

        // Метод для переключения на следующий меш
        public void NextItem(string categoryName)
        {
            var category = categories.Find(c => c.categoryName == categoryName);
            if (category == null) return;

            _currentIndices[categoryName] = (_currentIndices[categoryName] + 1) % category.availableMeshes.Count;
            UpdateAppearance(categoryName);
        }

        // Метод для переключения на предыдущий меш
        public void PreviousItem(string categoryName)
        {
            var category = categories.Find(c => c.categoryName == categoryName);
            if (category == null) return;

            _currentIndices[categoryName]--;
            if (_currentIndices[categoryName] < 0)
                _currentIndices[categoryName] = category.availableMeshes.Count - 1;

            UpdateAppearance(categoryName);
        }

        private void UpdateAppearance(string categoryName)
        {
            var category = categories.Find(c => c.categoryName == categoryName);
            int index = _currentIndices[categoryName];

            if (category != null && category.targetRenderer != null)
            {
                category.targetRenderer.sharedMesh = category.availableMeshes[index];
            }
        }

        public void ResetItems()
        {
            foreach (var c in categories)
            {
                _currentIndices[c.categoryName] = 0;
                UpdateAppearance(c.categoryName);
            }
        }

        public Dictionary<string, int> GetCharacterCustomization() =>
            new(_currentIndices);

        public void ApplyCharacter(Dictionary<string, int> indices)
        {
            foreach (var kv in indices)
            {
                _currentIndices[kv.Key] = kv.Value;
                UpdateAppearance(kv.Key);
            }
        }
    }
}