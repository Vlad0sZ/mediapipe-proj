using JetBrains.Annotations;
using R3;
using Runtime.Game.ScriptableData;

namespace Runtime.Game.Publishers
{
    [UsedImplicitly]
    public sealed class LevelPublisher : ILevelPublisher, ILevelPublisherSetup
    {
        private readonly Subject<GameSettings.Settings> _settingsSubject = new();
        public Observable<GameSettings.Settings> SettingsChanged => _settingsSubject;

        public void Publish(GameSettings.Settings settings) =>
            _settingsSubject.OnNext(settings);
    }
}