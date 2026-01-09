using Runtime.Game.ScriptableData;
using Runtime.Game.Types;

namespace Runtime.Game.Interfaces
{
    public interface IGameModeSettings
    {
        void SetupGameMode(GameMode gameMode);

        void SetLevelTime(float clamp01);

        float GetLevelTime();

        GameSettings.LevelSettings GetLevelTimeSettings();
        
        GameMode CurrentMode { get; }
    }
}