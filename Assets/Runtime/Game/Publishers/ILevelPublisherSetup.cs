using Runtime.Game.ScriptableData;

namespace Runtime.Game.Publishers
{
    public interface ILevelPublisherSetup
    {
        void Publish(GameSettings.Settings settings);
    }
}