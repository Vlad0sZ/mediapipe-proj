using R3;

namespace Runtime.Game.Interfaces
{
    public interface IPlayerRaisePublisher
    {
        Observable<PlayerPose> PlayerEvent { get; }
    }
}