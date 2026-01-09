using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Runtime.Infrastructure.Scenes;
using Runtime.UI.Interfaces;
using Runtime.UI.Screen;

namespace Runtime.Machine.States
{
    [UsedImplicitly]
    public sealed class LoadingGameState : UIState
    {
        private readonly ISceneLoader _sceneLoader;

        public LoadingGameState(ISceneLoader sceneLoader, ICanvas canvas) : base(canvas, ScreenNames.Loading) =>
            _sceneLoader = sceneLoader;

        public override async UniTask ActivateAsync(CancellationToken ct)
        {
            await base.ActivateAsync(ct);
            _sceneLoader.ChangeScene("Game Scene");
        }

        public override UniTask DeactivateAsync(CancellationToken ct) =>
            UniTask.CompletedTask;
    }
}