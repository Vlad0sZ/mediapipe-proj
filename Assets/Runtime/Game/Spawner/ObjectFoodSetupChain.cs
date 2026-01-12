using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Game.Interfaces;
using Runtime.Game.ScriptableData;
using Runtime.Infrastructure.Stacks;
using UnityEngine;

namespace Runtime.Game.Spawner
{
    public sealed class ObjectFoodSetupChain : ISpawnerChain, IDisposable
    {
        private readonly Dictionary<GameObject, GameObject> _parentWithChild = new();
        private readonly IStack<FoodWithIcon> _foodStack;
        private readonly Dictionary<FoodWithIcon, int> _foodPoints;

        public ObjectFoodSetupChain(IFoodGroupProvider foodGroupProvider)
        {
            var currentGroup = foodGroupProvider.GetCurrentFoodGroup();
            var food = currentGroup.Rights.Union(currentGroup.Wrong).ToArray();
            _foodStack = new ShuffledItemStack<FoodWithIcon>(food);

            var dictRights = currentGroup.Rights.ToDictionary(x => x, x => UnityEngine.Random.Range(1, 5));
            var dictWrongs = currentGroup.Wrong.ToDictionary(x => x, x => UnityEngine.Random.Range(-10, -5));
            _foodPoints = new Dictionary<FoodWithIcon, int>(dictRights.Union(dictWrongs));
        }

        public void OnSpawned(GameObject gameObject)
        {
            var nextModel = _foodStack.GetNext();

            if (_parentWithChild.TryGetValue(gameObject, out var child) == false)
            {
                child = UnityEngine.Object.Instantiate(nextModel.Prefab, gameObject.transform, false);
                child.transform.localPosition = Vector3.zero;
                _parentWithChild.Add(gameObject, child);
            }
            else
            {
                var mesh = child.GetComponent<MeshFilter>();
                var renderer = child.GetComponent<Renderer>();

                var prefab = nextModel.Prefab;

                renderer.sharedMaterial = prefab.GetComponent<Renderer>().sharedMaterial;
                mesh.sharedMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
            }

            if (gameObject.TryGetComponent<ICollectableItem>(out var collectable))
                collectable.Points = _foodPoints.GetValueOrDefault(nextModel, 0);
        }

        public void OnReleased(GameObject gameObject)
        {
            // no need to release object if mesh filter is changed rights;
        }

        public void Dispose()
        {
            foreach (KeyValuePair<GameObject, GameObject> valuePair in _parentWithChild)
            {
                if (valuePair.Value != null)
                    UnityEngine.Object.Destroy(valuePair.Value);
            }
        }
    }
}