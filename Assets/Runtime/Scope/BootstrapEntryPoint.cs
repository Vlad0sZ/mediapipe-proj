using JetBrains.Annotations;
using Runtime.Infrastructure.Scenes;
using Runtime.Infrastructure.Video;
using Runtime.UI.Interfaces;
using Runtime.UI.Screen;
using VContainer.Unity;

namespace Runtime.Scope
{
    [UsedImplicitly]
    public class BootstrapEntryPoint : IStartable
    {
        private readonly IWebCamInitializer _webCamInitializer;
        private readonly ISceneLoader _sceneLoader;
        private readonly ICanvas _canvas;

        public BootstrapEntryPoint(
            IWebCamInitializer webCamInitializer, ISceneLoader sceneLoader, ICanvas canvas)
        {
            _webCamInitializer = webCamInitializer;
            _sceneLoader = sceneLoader;
            _canvas = canvas;
        }

        public void Start()
        {
            var loadingScreen = _canvas.GetScreen(ScreenNames.Loading);
            loadingScreen?.Show();

            var isWebCamInitialized = _webCamInitializer.IsWebcamInitialized();
            if (isWebCamInitialized == false)
            {
                loadingScreen?.Hide();
                _canvas.GetScreen(ScreenNames.NoCamera)?.Show();
                return;
            }

            _sceneLoader.ChangeScene("Menu Scene");
        }
    }
}