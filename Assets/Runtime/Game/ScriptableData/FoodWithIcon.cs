using System;
using UnityEngine;

namespace Runtime.Game.ScriptableData
{
    [CreateAssetMenu(fileName = "Food", menuName = "Game/Food Object", order = 1)]
    public class FoodWithIcon : ScriptableObject
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private Sprite image;

        public GameObject Prefab => prefab;

        public Sprite Icon => image;

        public bool IsName(string foodName)
        {
            return this.name.Equals(foodName, StringComparison.OrdinalIgnoreCase)
                   || prefab.name.Equals(foodName, StringComparison.OrdinalIgnoreCase)
                   || image.name.Equals(foodName, StringComparison.OrdinalIgnoreCase);
        }
    }
}