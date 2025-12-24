using System;
using R3;
using Runtime.Game.Interfaces;
using Runtime.Game.Publishers;
using Runtime.Game.ScriptableData;
using VContainer;

namespace Runtime.Game
{
    public class ObjectSpeedSetup : ObjectSpawnerOwner
    {
        private CompositeDisposable _disposable;
        private GameSettings.ObjectsSettings _levelSettings;

        [Inject] private ILevelPublisher LevelPublisher { get; set; }

        private void OnEnable()
        {
            _disposable?.Dispose();
            _disposable = new CompositeDisposable();

            LevelPublisher.SettingsChanged
                .Subscribe(OnSettingsChanged)
                .AddTo(_disposable);

            ObjectSpawner.OnObjectSpawned.Subscribe(SetupObject)
                .AddTo(_disposable);
        }

        private void OnDisable() =>
            _disposable?.Dispose();

        private void SetupObject(ObjectSpawner.SpawnEvent spawnEvent)
        {
            if (spawnEvent is not ObjectSpawner.SpawnEvent.Created createdEvent)
                return;

            var obj = createdEvent.Object;
            var fallComponent = obj.GetComponent<IFallComponentSetup>();
            fallComponent.Setup(_levelSettings);
        }

        private void OnSettingsChanged(GameSettings.Settings settings) =>
            _levelSettings = settings.ObjectsSettings;
    }
}