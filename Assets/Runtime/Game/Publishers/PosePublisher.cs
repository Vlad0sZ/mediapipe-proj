using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using R3;

namespace Runtime.Game.Publishers
{
    [UsedImplicitly]
    public sealed class PosePublisher : IPosePublisher
    {
        private const int BodyCount = 2;
        private readonly object _lockObj = new();
        private readonly Subject<List<PlayerBody>> _subject = new Subject<List<PlayerBody>>();
        private readonly IPoseLandmarkPublisher _poseLandmarkPublisher;
        private readonly CompositeDisposable _disposable = new CompositeDisposable();
        private readonly List<PlayerBody> _playerBodies = new List<PlayerBody>(BodyCount);

        public Observable<List<PlayerBody>> Bodies => _subject;

        private PoseLandmarkerResult _result;

        public PosePublisher(IPoseLandmarkPublisher poseLandmarkPublisher)
        {
            _poseLandmarkPublisher = poseLandmarkPublisher;
            poseLandmarkPublisher.OnResult
                .Subscribe(CloneResult)
                .AddTo(_disposable);
        }

        public void Tick()
        {
            _subject.OnNext(_playerBodies.ToList());
        }

        private void CloneResult(PoseLandmarkerResult result)
        {
            _playerBodies.Clear();

            lock (_lockObj)
            {
                result.CloneTo(ref _result);

                for (int i = 0; i < BodyCount; i++)
                {
                    var norm = _result.poseLandmarks?.ElementAtOrDefault(i);
                    var world = _result.poseWorldLandmarks?.ElementAtOrDefault(i);

                    if (norm?.landmarks == null || world?.landmarks == null)
                        _playerBodies.Add(default);
                    else
                        _playerBodies.Add(new PlayerBody(i, world.Value, norm.Value));
                }
            }
        }

        public void Dispose()
        {
            _disposable?.Dispose();
            _playerBodies.Clear();
            _subject?.Dispose();
        }
    }
}