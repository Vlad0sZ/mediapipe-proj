using JetBrains.Annotations;
using R3;
using SensorPack.KinectCore.Runtime;

namespace Runtime.Game.Publishers
{
    [UsedImplicitly]
    public sealed class PosePublisher : IPosePublisher
    {
        private readonly Subject<KinectInterop.BodyData> _subject = new Subject<KinectInterop.BodyData>();
        public Observable<KinectInterop.BodyData> ActivePlayer => _subject;
        private readonly KinectManager _kinectManager;

        public PosePublisher(KinectManager kinectManager) =>
            _kinectManager = kinectManager;

        public void Tick()
        {
            if (_kinectManager.IsInitialized() == false)
                return;

            var body = _kinectManager.GetUserBodyDataByIndex(0);
            _subject.OnNext(body);
        }

        public void Dispose() =>
            _subject?.Dispose();
    }
}