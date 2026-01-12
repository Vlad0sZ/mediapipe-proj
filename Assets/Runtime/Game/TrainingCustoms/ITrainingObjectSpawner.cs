using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Game.Interfaces;
using Runtime.Game.ScriptableData;

namespace Runtime.Game.TrainingCustoms
{
    public interface ITrainingObjectSpawner
    {
        void Configure(FoodObjects.FoodGroup foodGroup);
        void ConfigureSpawner();
        
        UniTask<ICollectableItem> SpawnCorrectItemAsync(CancellationToken token);

        UniTask<ICollectableItem> SpawnWrongItemAsync(CancellationToken token);

        UniTask FreezeOnHeightAsync(ICollectableItem item, float targetY, CancellationToken token);
    }
}