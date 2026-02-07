using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace Runtime.CharacterPersonalization
{
    [System.Serializable]
    public class ClothingCategory
    {
        public string categoryName; // Название (например, "T-Shirts")
        public SkinnedMeshRenderer targetRenderer; // Объект на персонаже, где меняем меш
        public List<Mesh> availableMeshes; // Список доступных мешей для этой категории
    }
}