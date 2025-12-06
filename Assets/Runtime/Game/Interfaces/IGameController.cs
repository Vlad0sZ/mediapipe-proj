namespace Runtime.Game.Interfaces
{
    public interface IGameController
    {
        void SetupLevel();

        void StartLevel();

        void Pause();

        void Resume();

        void StopLevel();
    }
}