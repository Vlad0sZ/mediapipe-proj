using Runtime.Game.Models;
using Runtime.Game.ScriptableData;
using Runtime.Game.Types;

namespace Runtime.Game.Interfaces
{
    public interface IGameModeSettings
    {
        void SetupGameMode(GameMode gameMode);
        void SetPlayerName(string playerName);

        string GetPlayerName();
        
        void SetLevelTime(float clamp01);

        TimeModel GetLevelTime();

        GameSettings.Settings GetLevelSettings();

        GameMode CurrentMode { get; }
    }
}