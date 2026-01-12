using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Runtime.Game.Interfaces;
using Runtime.Game.ScriptableData;
using Runtime.Game.Spawner;

namespace Runtime.Game.TrainingCustoms
{
    [UsedImplicitly]
    public sealed class TrainingObjectSpawner : ITrainingObjectSpawner
    {
        private readonly IObjectSpawner _objectSpawner;
        private readonly ISpawnerSetup _spawnerSetup;
        private FoodObjects.FoodGroup _foodGroup;
        private ISpawnerChain _latestChain;

        public TrainingObjectSpawner(IObjectSpawner objectSpawner, ISpawnerSetup spawnerSetup)
        {
            _objectSpawner = objectSpawner;
            _spawnerSetup = spawnerSetup;
        }

        public void Configure(FoodObjects.FoodGroup foodGroup)
        {
            _foodGroup = foodGroup;
            _objectSpawner.Configure(new GameSettings.SpawnSettings()
            {
                maxObjectPerSpawn = 1,
                spawnDelay = 3,
            });
        }

        public void ConfigureSpawner()
        {
            if (_latestChain != null)
                _spawnerSetup.RemoveSpawnerChain(_latestChain);
        }

        public async UniTask<ICollectableItem> SpawnCorrectItemAsync(CancellationToken token)
        {
            _objectSpawner.Configure(new GameSettings.SpawnSettings()
            {
                maxObjectPerSpawn =  1,
                spawnDelay = 3,
            });
            
            return await SpawnSingleItemAsync(positive: true, token);
        }

        public async UniTask<ICollectableItem> SpawnWrongItemAsync(CancellationToken token)
        {
            _objectSpawner.Configure(new GameSettings.SpawnSettings()
            {
                maxObjectPerSpawn =  1,
                spawnDelay = 7,
            });
            
            return await SpawnSingleItemAsync(positive: false, token);
        }

        public async UniTask FreezeOnHeightAsync(ICollectableItem item, float targetY, CancellationToken token)
        {
            if (item is not IGameObject goWrapper)
                return;

            var go = goWrapper.gameObject;

            do
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            } while (!token.IsCancellationRequested &&
                     go != null &&
                     go.transform.position.y > targetY);

            if (token.IsCancellationRequested || go == null)
                return;

            _objectSpawner.Pause();
        }

        private async UniTask<ICollectableItem> SpawnSingleItemAsync(bool positive, CancellationToken token)
        {
            var tcs = new UniTaskCompletionSource<ICollectableItem>();

            // Выбираем нужную еду
            var foodStack = positive ? _foodGroup.Rights : _foodGroup.Wrong;
            var food = GetRandomFood(foodStack);

            if (food == null || food.Prefab == null)
            {
                tcs.TrySetException(new InvalidOperationException("Food prefab is null"));
                return await tcs.Task;
            }

            if (_latestChain != null)
                _spawnerSetup.RemoveSpawnerChain(_latestChain);

            var points = positive ? 1 : -1;
            _latestChain = new TrainingObjectChain(points, food.Prefab, tcs);
            _spawnerSetup.AddSpawnerChain(_latestChain);

            _objectSpawner.Resume();
            _objectSpawner.StartSpawn();
            await using (token.Register(() => tcs.TrySetCanceled()))
            {
                var result = await tcs.Task;
                return result;
            }
        }


        private static FoodWithIcon GetRandomFood(IReadOnlyList<FoodWithIcon> list)
        {
            if (list == null || list.Count == 0)
                return null;

            var index = UnityEngine.Random.Range(0, list.Count);
            return list[index];
        }
    }
}