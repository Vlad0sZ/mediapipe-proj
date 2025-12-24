namespace Runtime.Game.Embient
{
    public interface ICameraController
    {
        void LiveMainCamera();

        void LiveLevelCamera(int level);
    }
}