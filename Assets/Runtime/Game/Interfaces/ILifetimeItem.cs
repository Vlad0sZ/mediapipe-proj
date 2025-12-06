using R3;

namespace Runtime.Game.Interfaces
{
    public interface ILifetimeItem : IGameObject
    {
        void SetLifetime(float seconds);

        Observable<ILifetimeItem> LifetimeObservable { get; }
    }
}