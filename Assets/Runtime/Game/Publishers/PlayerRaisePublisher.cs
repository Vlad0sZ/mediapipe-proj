using System;
using System.Collections.Generic;
using System.Linq;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using R3;
using Runtime.Game.Interfaces;
using Runtime.Game.Publishers;
using Runtime.Types;
using UnityEngine;
using VContainer;
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
            _posePublisher.Bodies
                .Subscribe(OnPoseDetected)
                .AddTo(_subscription);

        private void OnPoseDetected(List<PlayerBody> playerBodies)
        {
            var firstPlayer = playerBodies.FirstOrDefault(x => x != null);
            var handGesture = GetHandGesture(firstPlayer);
            _subject.OnNext(new PlayerPose(firstPlayer != null, handGesture));
        }

        private static HandRaiseType GetHandGesture(PlayerBody firstPlayer)
        {
            if (firstPlayer == null)
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