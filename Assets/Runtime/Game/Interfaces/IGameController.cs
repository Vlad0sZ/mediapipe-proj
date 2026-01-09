namespace Runtime.Game.Interfaces
{
    public interface IGameController
    {
        void StartLevel();

        void Pause();

        void Resume();

        void StopLevel();
    }
}