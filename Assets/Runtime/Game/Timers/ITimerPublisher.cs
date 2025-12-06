using R3;

namespace Runtime.Game.Timers
{
    public interface ITimerPublisher
    {
        Observable<ElapsedTime> Event { get; }
    }
}