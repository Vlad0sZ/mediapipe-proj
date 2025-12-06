using System;
using UnityEngine;

namespace Runtime.Game
{
    [System.Serializable]
    public class FoodModel
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int points;


        public GameObject Prefab => prefab;

        public int Points => points;
    }
}