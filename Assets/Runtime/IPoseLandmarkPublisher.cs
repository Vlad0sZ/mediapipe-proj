using Mediapipe.Tasks.Vision.PoseLandmarker;
using R3;

namespace Runtime
{
    public interface IPoseLandmarkPublisher
    {
        Observable<PoseLandmarkerResult> OnResult { get; }

        void Restart();
    }
}