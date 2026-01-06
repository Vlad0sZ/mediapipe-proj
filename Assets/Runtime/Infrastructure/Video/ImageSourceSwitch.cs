using SensorPack.Addons.Mediapipe.Solutions.Runners.PoseRunners;

namespace Runtime.Infrastructure.Video
{
    public class ImageSourceSwitch : IImageSourceProvider
    {
        private readonly PoseSolution _publisher;

        public ImageSourceSwitch(PoseSolution publisher) =>
            _publisher = publisher;

        public void ChangeSource(int index)
        {
            _publisher.Stop();
            _publisher.ImageSource.SelectSource(index);
            _publisher.Play();
        }
    }
}