using Runtime.Game.Interfaces;
using Runtime.Game.ScriptableData;
using VContainer.Unity;

namespace Runtime.Game.Timers
{
    public interface ITimer : ITickable, ITimerPublisher
    {
        void StartTimer(float seconds);
        void StopTimer();
        void Pause();
        void Resume();
    }
}