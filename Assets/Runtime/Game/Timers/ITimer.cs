using Runtime.Game.Interfaces;
using Runtime.Game.ScriptableData;
using VContainer.Unity;

namespace Runtime.Game.Timers
{
    public interface ITimer : ITickable, ITimerPublisher
    {
        void StartTimer();
        void StopTimer();
        void Pause();
        void Resume();
    }
}