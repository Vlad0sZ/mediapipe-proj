using Runtime.Game.Spawner;

namespace Runtime.Game
{
    public interface ISpawnerSetup
    {
        void AddSpawnerChain(ISpawnerChain chain);
        void RemoveSpawnerChain(ISpawnerChain chain);
    }
}