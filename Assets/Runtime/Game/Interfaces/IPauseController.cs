using R3;

namespace Runtime.Game.Interfaces
{
    public interface IPauseController
    {
        Observable<bool> OnPaused { get; }
        void StartControl();
        void StopControl();
    }
}