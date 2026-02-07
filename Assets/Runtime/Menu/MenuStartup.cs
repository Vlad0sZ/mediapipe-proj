using Runtime.Infrastructure.Scenes;
using UnityEngine;
using VContainer;

namespace Runtime.Menu
{
    public class MenuStartup : MonoBehaviour
    {
        private Coroutine _coroutine;
        private ISceneLoader _sceneLoader;

        [Inject]
        public void Construct(ISceneLoader sceneLoader) => 
            _sceneLoader = sceneLoader;

        public void LoadGameScene() =>
            _sceneLoader.ChangeScene("Game Scene");
    }
}