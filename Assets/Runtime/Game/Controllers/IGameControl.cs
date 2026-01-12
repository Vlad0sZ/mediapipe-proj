using R3;
using Runtime.Game.Interfaces;

namespace Runtime.Game.Controllers
{
    public interface IGameControl
    {
        Observable<Unit> EndGame { get; }
        void OnStart(IGameModeSettings settings);
        void OnPaused();
        void OnResumed();
        void OnStopped();
    }
}