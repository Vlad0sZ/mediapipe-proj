using JetBrains.Annotations;
using Runtime.Infrastructure.Scenes;
using Runtime.UI.Interfaces;
using Runtime.UI.Screen;

namespace Runtime.Machine.States
{
    [UsedImplicitly]
    public sealed class LoadingGameState : IState
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly ICanvas _canvas;

        public LoadingGameState(ISceneLoader sceneLoader, ICanvas canvas)
        {
            _sceneLoader = sceneLoader;
            _canvas = canvas;
        }

        public void Activate()
        {
            _canvas.GetScreen(ScreenNames.Loading)?.Show();
            _sceneLoader.ChangeScene("Game Scene");
        }

        public void Deactivate()
        {
        }
    }
}