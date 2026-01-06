using System;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using R3;
using Runtime.Game.Interfaces;
using Runtime.Game.Publishers;
using Runtime.Types;
using SensorPack.KinectCore.Runtime;
using VContainer.Unity;

namespace Runtime.Game
{
    public class PlayerRaisePublisher : IPlayerRaisePublisher, IStartable, IDisposable
    {
        private readonly Subject<PlayerPose> _subject = new Subject<PlayerPose>();
        private readonly CompositeDisposable _subscription = new CompositeDisposable();
        private readonly IPosePublisher _posePublisher;

        private PoseLandmarkerResult _latestResult;

        public PlayerRaisePublisher(IPosePublisher posePublisher) => _posePublisher = posePublisher;

        public Observable<PlayerPose> PlayerEvent => _subject;

        public void Start() =>
            _posePublisher.ActivePlayer
                .Subscribe(OnPoseDetected)
                .AddTo(_subscription);

        private void OnPoseDetected(KinectInterop.BodyData body)
        {
            var handGesture = GetHandGesture(body);
            _subject.OnNext(new PlayerPose(body.bIsTracked > 0, handGesture));
        }

        private static HandRaiseType GetHandGesture(KinectInterop.BodyData firstPlayer)
        {
            if (firstPlayer.bIsTracked == 0)
                return HandRaiseType.None;


            var headPos = firstPlayer.GetNormalizedCoordinate(JointType.Nose, Coordinates.Y);
            var mouthPos = firstPlayer.GetNormalizedCoordinate(JointType.LeftMouth, Coordinates.Y);

            bool isUpsideDown = mouthPos > headPos;

            var leftElbowPos = firstPlayer.GetNormalizedCoordinate(JointType.LeftElbow, Coordinates.Y);
            var rightElbowPos = firstPlayer.GetNormalizedCoordinate(JointType.RightElbow, Coordinates.Y);
            var leftHandPos = firstPlayer.GetNormalizedCoordinate(JointType.LeftWrist, Coordinates.Y);
            var rightHandPos = firstPlayer.GetNormalizedCoordinate(JointType.RightWrist, Coordinates.Y);

            bool isLeftUp = isUpsideDown
                ? leftHandPos < leftElbowPos && leftElbowPos < headPos
                : leftHandPos > leftElbowPos && leftElbowPos > headPos;


            bool isRightUp = isUpsideDown
                ? rightHandPos < rightElbowPos && rightElbowPos < headPos
                : rightHandPos > rightElbowPos && rightElbowPos > headPos;

            var leftGesture = isLeftUp ? HandRaiseType.LeftHandRaised : HandRaiseType.LeftHandBelow;
            var rightGesture = isRightUp ? HandRaiseType.RightHandRaised : HandRaiseType.RightHandBelow;

            return leftGesture | rightGesture;
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subject?.Dispose();
        }
    }
}