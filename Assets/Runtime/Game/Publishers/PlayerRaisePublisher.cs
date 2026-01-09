using System;
using JetBrains.Annotations;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using R3;
using Runtime.Game.Interfaces;
using Runtime.Types;
using SensorPack.KinectCore.Runtime;
using VContainer.Unity;

namespace Runtime.Game.Publishers
{
    [UsedImplicitly]
    public sealed class PlayerRaisePublisher : IPlayerRaisePublisher, IStartable, IDisposable
    {
        private readonly Subject<PlayerPose> _subject = new Subject<PlayerPose>();
        private readonly CompositeDisposable _subscription = new CompositeDisposable();
        private readonly IPosePublisher _posePublisher;
        private readonly KinectManager _kinectManager;


        private PoseLandmarkerResult _latestResult;

        public PlayerRaisePublisher(IPosePublisher posePublisher, KinectManager kinectManager)
        {
            _posePublisher = posePublisher;
            _kinectManager = kinectManager;
        }

        public Observable<PlayerPose> PlayerEvent => _subject;

        public void Start() =>
            _posePublisher.ActivePlayer
                .Subscribe(OnPoseDetected)
                .AddTo(_subscription);

        private void OnPoseDetected(KinectInterop.BodyData body)
        {
            var isHorizontalFlipped = _kinectManager.GetColorImageScale().x < 0;
            var handGesture = GetHandGesture(body, isHorizontalFlipped);
            UnityEngine.Debug.Log($"next pose = {body.bIsTracked} + {handGesture}");
            _subject.OnNext(new PlayerPose(body.bIsTracked > 0, handGesture));
        }

        private static HandRaiseType GetHandGesture(KinectInterop.BodyData firstPlayer, bool isHorizontalFlipped)
        {
            if (firstPlayer.bIsTracked == 0)
                return HandRaiseType.None;

            var headPos = firstPlayer.GetNormalizedCoordinate(KinectInterop.JointType.Head, Coordinates.Y);
            var mouthPos = firstPlayer.GetNormalizedCoordinate(KinectInterop.JointType.Neck, Coordinates.Y);

            bool isUpsideDown = mouthPos > headPos;

            var leftElbowPos = firstPlayer.GetNormalizedCoordinate(KinectInterop.JointType.ElbowLeft, Coordinates.Y);
            var rightElbowPos = firstPlayer.GetNormalizedCoordinate(KinectInterop.JointType.ElbowRight, Coordinates.Y);
            var leftHandPos = firstPlayer.GetNormalizedCoordinate(KinectInterop.JointType.HandLeft, Coordinates.Y);
            var rightHandPos = firstPlayer.GetNormalizedCoordinate(KinectInterop.JointType.HandRight, Coordinates.Y);

            bool isLeftUp = isUpsideDown
                ? leftHandPos < leftElbowPos && leftElbowPos < headPos
                : leftHandPos > leftElbowPos && leftElbowPos > headPos;

            bool isRightUp = isUpsideDown
                ? rightHandPos < rightElbowPos && rightElbowPos < headPos
                : rightHandPos > rightElbowPos && rightElbowPos > headPos;

            if (!isHorizontalFlipped)
                (isLeftUp, isRightUp) = (isRightUp, isLeftUp);

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