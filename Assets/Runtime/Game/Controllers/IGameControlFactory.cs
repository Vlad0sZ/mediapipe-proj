using Runtime.Game.Types;

namespace Runtime.Game.Controllers
{
    public interface IGameControlFactory
    {
        IGameControl GenerateGameControl(GameMode gameMode);
    }
}