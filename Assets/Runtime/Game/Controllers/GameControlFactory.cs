using Runtime.Game.Types;
using VContainer;

namespace Runtime.Game.Controllers
{
    public sealed class GameControlFactory : IGameControlFactory
    {
        private readonly IObjectResolver _objectResolver;

        public GameControlFactory(IObjectResolver objectResolver) =>
            _objectResolver = objectResolver;

        public IGameControl GenerateGameControl(GameMode gameMode) =>
            _objectResolver.Resolve<IGameControl>(gameMode);
    }
}