using System;
using JetBrains.Annotations;
using R3;
using Runtime.Game.Interfaces;
using Runtime.Game.Spawner;
using Runtime.Game.Timers;

namespace Runtime.Game.Controllers
{
    [UsedImplicitly]
    public sealed class ClassicGameControl : IGameControl
    {
        private readonly ITimer _timer;
        private readonly IObjectSpawner _objectSpawner;
        private readonly ISpawnerSetup _spawnerSetup;
        private readonly IFoodGroupProvider _foodGroupProvider;
        private readonly Subject<Unit> _onEndGame = new();

        private IDisposable _timerSubscription;
        private ISpawnerChain _spawnerChain;
        private ISpawnerChain _foodChain;

        public ClassicGameControl(ITimer timer, IObjectSpawner objectSpawner, ISpawnerSetup spawnerSetup,
            IFoodGroupProvider foodGroupProvider)
        {
            _timer = timer;
            _objectSpawner = objectSpawner;
            _spawnerSetup = spawnerSetup;
            _foodGroupProvider = foodGroupProvider;
        }

        public Observable<Unit> EndGame => _onEndGame;

        public void OnStart(IGameModeSettings settings)
        {
            var (_, spawnSettings, speedSettings) = settings.GetLevelSettings();

            _spawnerChain = new ObjectSpeedSetupChain(speedSettings);
            _foodChain = new ObjectFoodSetupChain(_foodGroupProvider);
            _spawnerSetup.AddSpawnerChain(_foodChain);
            _spawnerSetup.AddSpawnerChain(_spawnerChain);
            _objectSpawner.Configure(spawnSettings);

            _timerSubscription = _timer.Event.Subscribe(OnTimerEvent);
            _timer.StartTimer(settings.GetLevelTime().TotalSeconds);
            _objectSpawner.StartSpawn();
        }

        public void OnPaused()
        {
            _timer.Pause();
            _objectSpawner.StopSpawn();
        }

        public void OnResumed()
        {
            _timer.Resume();
            _objectSpawner.StartSpawn();
        }

        public void OnStopped()
        {
            _timerSubscription?.Dispose();
            _timer.StopTimer();
            _objectSpawner.StopSpawn();
            _spawnerSetup.RemoveSpawnerChain(_spawnerChain);
            _spawnerSetup.RemoveSpawnerChain(_foodChain);
        }

        private void OnTimerEvent(ElapsedTime model)
        {
            if (model.Progress < 1f)
                return;


            _onEndGame.OnNext(default);
        }
    }
}