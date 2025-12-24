using R3;
using Runtime.Game.ScriptableData;

namespace Runtime.Game.Publishers
{
    public interface ILevelPublisher
    {
        Observable<GameSettings.Settings> SettingsChanged { get; }
    }
}