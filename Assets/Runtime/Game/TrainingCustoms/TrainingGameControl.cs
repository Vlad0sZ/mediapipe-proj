using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using Runtime.Game.Controllers;
using Runtime.Game.Interfaces;
using Runtime.Game.ScriptableData;
using Runtime.Game.Spawner;
using Runtime.Infrastructure.Interfaces;
using Runtime.Types;
using UnityEngine;
using ITimer = Runtime.Game.Timers.ITimer;

namespace Runtime.Game.TrainingCustoms
{
    [UsedImplicitly]
    public sealed class TrainingGameControl : IGameControl
    {
        private readonly ITimer _timer;
        private readonly IObjectSpawner _objectSpawner;
        private readonly ITrainingObjectSpawner _trainingSpawner;
        private readonly IPlayerRaisePublisher _playerRaisePublisher;
        private readonly ITrainingUI _trainingUi;
        private readonly ICoroutineScope _coroutineScope;
        private readonly IFoodGroupProvider _foodGroupProvider;
        private readonly ISpawnerSetup _spawnerSetup;

        private readonly Subject<Unit> _endGameSub = new();
        private readonly ISpawnerChain _foodChain;

        private IDisposable _timerSubscription;
        private CancellationTokenSource _cts;
        private Coroutine _trainingRoutine;
        private bool _isTrainingFinished;

        private ISpawnerChain _speedChain;
        private GameSettings.SpawnSettings _spawnSettings;

        public TrainingGameControl(
            ITimer timer,
            ITrainingObjectSpawner trainingSpawner,
            IPlayerRaisePublisher playerRaisePublisher,
            ITrainingUI trainingUi,
            ICoroutineScope coroutineScope,
            IFoodGroupProvider foodGroupProvider,
            IObjectSpawner objectSpawner, ISpawnerSetup spawnerSetup)
        {
            _timer = timer;
            _trainingSpawner = trainingSpawner;
            _playerRaisePublisher = playerRaisePublisher;
            _trainingUi = trainingUi;
            _coroutineScope = coroutineScope;
            _foodGroupProvider = foodGroupProvider;
            _objectSpawner = objectSpawner;
            _spawnerSetup = spawnerSetup;
            _foodChain = new ObjectFoodSetupChain(_foodGroupProvider);
        }

        public Observable<Unit> EndGame => _endGameSub;

        public void OnStart(IGameModeSettings settings)
        {
            _cts = new CancellationTokenSource();

            var levelSettings = settings.GetLevelSettings();
            _spawnSettings = levelSettings.SpawnSettings;
            var foodGroup = _foodGroupProvider.GetCurrentFoodGroup();

            _speedChain = new ObjectSpeedSetupChain(levelSettings.ObjectsSettings);
            _spawnerSetup.AddSpawnerChain(_speedChain);
            _trainingSpawner.Configure(foodGroup);

            // Сначала запускаем таймер, но сразу ставим на паузу во время шагов обучения.
            _timerSubscription = _timer.Event.Subscribe(OnTimerEvent);
            _trainingRoutine =
                _coroutineScope.StartCoroutine(TrainingFlowAsync(_cts.Token).ToCoroutine(Debug.LogError));
        }

        public void OnPaused()
        {
            if (!_isTrainingFinished) return;
            _timer.Pause();
            _objectSpawner.Pause();
        }

        public void OnResumed()
        {
            if (!_isTrainingFinished) return;
            _objectSpawner.Resume();
            _timer.Resume();
        }


        public void OnStopped()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            _timerSubscription?.Dispose();
            _timerSubscription = null;

            if (_trainingRoutine != null)
                _coroutineScope.StopCoroutine(_trainingRoutine);

            _objectSpawner.StopSpawn();
            _timer.StopTimer();

            _trainingUi.HideRaiseHandsHint();
            _trainingUi.HideCollectHint(false);
            _trainingUi.HideCollectHint(true);
            _trainingUi.HideHints();

            _spawnerSetup.RemoveSpawnerChain(_foodChain);
            _spawnerSetup.RemoveSpawnerChain(_speedChain);
        }

        private void OnTimerEvent(ElapsedTime model)
        {
            if (model.Progress < 1f)
                return;

            _endGameSub.OnNext(Unit.Default);
        }


        private async UniTask TrainingFlowAsync(CancellationToken token)
        {
            try
            {
                // Шаг 1 — поднять руки
                await StepRaiseHandsAsync(token);

                // Шаг 2 — правильный объект
                await StepCollectCorrectItemAsync(token);

                // Шаг 3 — неправильный объект
                await StepCollectWrongItemAsync(token);

                // Шаг 4 — подсказки про очки и таймер, запуск основного игрового спавна
                await StepScoreAndTimerAsync(token);
            }
            catch (OperationCanceledException)
            {
                // игнор
            }
        }

        private async UniTask StepRaiseHandsAsync(CancellationToken token)
        {
            _trainingUi.ShowRaiseHandsHint();

            using var handler = new RaiseHandAwaitable(_playerRaisePublisher,
                HandRaiseType.LeftHandRaised | HandRaiseType.RightHandBelow, 0.5f);
            await handler.WaitForRaiseType(token);
            _trainingUi.HideRaiseHandsHint();
        }

        private async UniTask StepCollectCorrectItemAsync(CancellationToken token)
        {
            // Спавним правильный объект
            var tcs = new UniTaskCompletionSource();
            var item = await _trainingSpawner.SpawnCorrectItemAsync(token);
            IDisposable collectSub =  item.CollectableSubject.Subscribe(_ => { tcs.TrySetResult(); });

            _trainingSpawner.FreezeOnHeightAsync(item, targetY: 1.5f, token).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(1.5f), DelayType.Realtime, PlayerLoopTiming.Update, token);
            _trainingUi.ShowCollectHintOnItem(item, true);
            try
            {
                await using (token.Register(() => tcs.TrySetCanceled()))
                    await tcs.Task;


                _trainingUi.HideCollectHint(true);
            }
            finally
            {
                collectSub?.Dispose();
            }
        }

        private async UniTask StepCollectWrongItemAsync(CancellationToken token)
        {
            var tcs = new UniTaskCompletionSource();
            var item = await _trainingSpawner.SpawnWrongItemAsync(token);
            var collectSub = item.CollectableSubject.Subscribe(_ => tcs.TrySetResult());

            _trainingSpawner.FreezeOnHeightAsync(item, targetY: 1f, token).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(2.5f), DelayType.Realtime, PlayerLoopTiming.Update, token);
            _trainingUi.ShowCollectHintOnItem(item, false);


            try
            {
                await using (token.Register(() => tcs.TrySetCanceled()))
                    await tcs.Task;

                _trainingUi.HideCollectHint(false);
            }
            finally
            {
                collectSub.Dispose();
            }
        }

        private async UniTask StepScoreAndTimerAsync(CancellationToken token)
        {
            _trainingSpawner.ConfigureSpawner();

            _trainingUi.ShowScoreHint();
            await UniTask.Delay(TimeSpan.FromSeconds(8), DelayType.DeltaTime, PlayerLoopTiming.Update, token);

            _trainingUi.ShowTimerHint();
            await UniTask.Delay(TimeSpan.FromSeconds(5), DelayType.DeltaTime, PlayerLoopTiming.Update, token);

            _trainingUi.ShowFinishHint();

            using var handler = new RaiseHandAwaitable(_playerRaisePublisher,
                HandRaiseType.LeftHandBelow | HandRaiseType.RightHandRaised, 0.5f);

            await handler.WaitForRaiseType(token);
            _trainingUi.HideHints();

            _timer.StartTimer(25f);
            _isTrainingFinished = true;

            _spawnerSetup.AddSpawnerChain(_foodChain);
            _objectSpawner.Configure(_spawnSettings);
            _objectSpawner.StartSpawn();
        }


        private sealed class RaiseHandAwaitable : IDisposable
        {
            private readonly UniTaskCompletionSource _tcs;
            private readonly IPlayerRaisePublisher _raisePublisher;
            private readonly IDisposable _disposable;
            private readonly HandRaiseType _raiseType;
            private readonly float _raiseTime;
            private float _holdTime = 0f;

            public RaiseHandAwaitable(IPlayerRaisePublisher raisePublisher, HandRaiseType raiseType, float raiseTime)
            {
                _tcs = new UniTaskCompletionSource();
                _disposable = raisePublisher.PlayerEvent.Subscribe(OnEvent);
                _raiseTime = raiseTime;
                _raiseType = raiseType;
            }

            private void OnEvent(PlayerPose pose)
            {
                if (!pose.IsVisible)
                    return;

                if (pose.HandRaiseType == _raiseType)
                    _holdTime += Time.deltaTime;
                else
                    _holdTime = 0f;

                if (_holdTime >= _raiseTime)
                    _tcs.TrySetResult();
            }


            public async UniTask WaitForRaiseType(CancellationToken token)
            {
                await using (token.Register(() => _tcs.TrySetCanceled()))
                    await _tcs.Task;
            }

            public void Dispose() =>
                _disposable?.Dispose();
        }
    }
}