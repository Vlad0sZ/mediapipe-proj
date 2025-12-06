using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Runtime.Game.Interfaces;
using Runtime.Game.ScriptableData;
using Runtime.Infrastructure.Stacks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Game
{
    public class FoodFactory : ObjectSpawnerOwner, IFoodFactory
    {
        private readonly Dictionary<GameObject, GameObject> _parentWithChild = new();
        private IDisposable _disposable;

        private IStack<FoodWithIcon> _allFood;
        private Dictionary<FoodWithIcon, int> _foodPoints;

        private void OnEnable()
        {
            var createSub = ObjectSpawner.OnObjectSpawned
                .Where(u => u is ObjectSpawner.SpawnEvent.Created)
                .Select(u => u.Object)
                .Subscribe(SetupFood);

            var releaseSub = ObjectSpawner.OnObjectSpawned
                .Where(u => u is ObjectSpawner.SpawnEvent.Released)
                .Select(u => u.Object)
                .Subscribe(RemoveFood);


            _disposable = Disposable.Combine(createSub, releaseSub);
        }

        private void OnDisable() =>
            _disposable?.Dispose();


        
        public void Setup(FoodObjects.FoodGroup payload)
        {
            var rights = payload.Rights;
            var wrong = payload.Wrong;

            var allFood = rights.Union(wrong).ToArray();
            _allFood = new ShuffledItemStack<FoodWithIcon>(allFood);
            _foodPoints = allFood.ToDictionary(x => x,
                x => rights.Contains(x) ? Random.Range(1, 5) : Random.Range(-10, -5));
        }

        private void SetupFood(GameObject obj)
        {
            var nextModel = _allFood?.GetNext();

            if (nextModel == null || nextModel.Prefab == null)
                return;

            var child = Instantiate(nextModel.Prefab, obj.transform, false);
            child.transform.localPosition = Vector3.zero;

            if (obj.TryGetComponent<ICollectableItem>(out var collectable))
                collectable.Points = _foodPoints.GetValueOrDefault(nextModel, 0);

            if (_parentWithChild.ContainsKey(obj))
                RemoveFood(obj);

            _parentWithChild.Add(obj, child);
        }

        private void RemoveFood(GameObject obj)
        {
            if (_parentWithChild.TryGetValue(obj, out var child))
                Destroy(child);

            _parentWithChild.Remove(obj);
        }

    }
}