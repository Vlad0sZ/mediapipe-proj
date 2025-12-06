using System;
using System.Collections;
using Runtime.Infrastructure.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runtime.Infrastructure.Scenes
{
    public sealed class SceneLoader : ISceneLoader
    {
        private readonly ICoroutineScope _coroutineScope;
        private Coroutine _coroutine;

        public SceneLoader(ICoroutineScope coroutineScope) =>
            _coroutineScope = coroutineScope;


        public void ChangeScene(string scene, Action callback = null)
        {
            if (_coroutine != null)
                _coroutineScope.StopCoroutine(_coroutine);

            _coroutine = _coroutineScope.StartCoroutine(LoadScene(scene, callback));
        }

        private static IEnumerator LoadScene(string sceneName, Action callback)
        {
            var currentScene = SceneManager.GetActiveScene();
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!operation.isDone)
            {
                if (operation.progress >= 0.9f)
                    operation.allowSceneActivation = true;
                yield return null;
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            yield return SceneManager.UnloadSceneAsync(currentScene);
            callback?.Invoke();
        }
    }
}