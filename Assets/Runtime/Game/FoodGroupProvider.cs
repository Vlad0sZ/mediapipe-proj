using JetBrains.Annotations;
using Runtime.Game.Interfaces;
using Runtime.Game.ScriptableData;

namespace Runtime.Game
{
    [UsedImplicitly]
    public sealed class FoodGroupProvider : IFoodGroupProvider, IFoodPayload
    {
        private FoodObjects.FoodGroup _foodGroup;

        public FoodObjects.FoodGroup GetCurrentFoodGroup() =>
            _foodGroup;

        public void Setup(FoodObjects.FoodGroup payload) =>
            _foodGroup = payload;
    }
}