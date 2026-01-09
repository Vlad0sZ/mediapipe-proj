using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Runtime.Infrastructure.Interfaces;
using UnityEngine;

namespace Runtime.Machine.States
{
    [UsedImplicitly]
    public sealed class ExitGameState : IAsyncState
    {
        public async UniTask ActivateAsync(CancellationToken ct)
        {
            await UniTask.Delay(300, cancellationToken: ct);
            Application.Quit();
        }

        public UniTask DeactivateAsync(CancellationToken ct) =>
            UniTask.CompletedTask;
    }
}