using Runtime.Game.ScriptableData;

namespace Runtime.Game.Interfaces
{
    public interface IGameModePayload : ISetupPayload<IGameModeSettings>
    {
        void SetupFood(FoodObjects.FoodGroup foodGroup);

    }
}