using Runtime.Game.ScriptableData;

namespace Runtime.Game.Interfaces
{
    public interface IFoodGroupProvider
    {
        FoodObjects.FoodGroup GetCurrentFoodGroup();
    }
}