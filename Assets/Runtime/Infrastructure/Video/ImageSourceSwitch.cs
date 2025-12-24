using Mediapipe.Unity.Sample;

namespace Runtime.Infrastructure.Video
{
    public class ImageSourceSwitch : IImageSourceProvider
    {
        private readonly IPoseLandmarkPublisher _publisher;

        public ImageSourceSwitch(IPoseLandmarkPublisher publisher) =>
            _publisher = publisher;

        public void ChangeSource(int index)
        {
            ImageSourceProvider.ImageSource.SelectSource(index);
            _publisher.Restart();
        }
    }
}