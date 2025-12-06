using R3;

namespace Runtime.Game.Interfaces
{
    public interface ICollectableItem : IGameObject
    {
        Observable<ICollectableItem> CollectableSubject { get; }
        int Points { get; set; }
        void Collect();


    }
}