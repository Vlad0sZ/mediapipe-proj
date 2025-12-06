using System;

namespace Runtime.Infrastructure.Scenes
{
    public interface ISceneLoader
    {
        void ChangeScene(string scene, Action callback = null);
    }
}