using System.Collections.Generic;
using System.Linq;
using R3;
using Runtime.Game.Publishers;
using UnityEngine;
using VContainer;

namespace Runtime
{
    public class ArmController : MonoBehaviour
    {
        public Transform target;
        public Transform hint;

        public Transform elbowBone;
        public Transform wristBone;
        public Vector3 rotation;

        public JointType wristType;
        public JointType elbowType;
        public JointType shoulderType;

        public float offset = 1f;
        public float smoothDistance;
        public float gizmoSize = 1f;

        private readonly Vector3[] _latestPositions = new Vector3[3];
        private readonly Vector3[] _gizmosPos = new Vector3[2];
        private readonly CompositeDisposable _subscription = new CompositeDisposable();
        private IPosePublisher _posePublisher;
        private float _forearmLen;

        private Vector3 _defaultElbowPos;
        private Vector3 _defaultWristPos;


        [Inject]
        public void Construct(IPosePublisher posePublisher)
        {
            _posePublisher = posePublisher;
        }

        private void Start()
        {
            var elbow = elbowBone.position;
            _forearmLen = Vector3.Distance(elbow, wristBone.position);

            _defaultElbowPos = hint.position;
            _defaultWristPos = target.position;

            _posePublisher.Bodies.Subscribe(OnPose)
                .AddTo(_subscription);
        }

        private void OnDestroy() =>
            _subscription.Dispose();

        private void OnPose(List<PlayerBody> obj)
        {
            var firstPlayer = obj.FirstOrDefault(x => x != null && x.IsExists());
            if (firstPlayer == null)
                return;

            ApplyArm(firstPlayer);
        }

        private void ApplyArm(PlayerBody pose)
        {
            var shoulderPos = GetSmoothPosition(pose, shoulderType, 0);
            var elbowPos = GetSmoothPosition(pose, elbowType, 1);
            var wristPos = GetSmoothPosition(pose, wristType, 2);


            if (elbowPos == Vector3.zero || wristPos == Vector3.zero)
            {
                hint.position = _defaultElbowPos;
                target.position = _defaultWristPos;
            }

            var skeletonLength = _forearmLen;
            var humanLength = Vector3.Distance(shoulderPos, elbowPos);
            var diff = skeletonLength / humanLength;

            var upperArmDir = (elbowPos - shoulderPos) * diff;
            var forearmDir = (wristPos - elbowPos) * diff;

            var elbowTarget = elbowBone.position + upperArmDir;
            var wristTarget = elbowTarget + forearmDir;

            _gizmosPos[0] = elbowTarget * offset;
            _gizmosPos[1] = wristTarget * offset;

            hint.position = elbowTarget * offset;
            target.position = wristTarget * offset;
            target.localRotation = Quaternion.FromToRotation(upperArmDir, forearmDir) * Quaternion.Euler(rotation);
        }

        private Vector3 GetSmoothPosition(PlayerBody posePosition, JointType index, int i)
        {
            var position = posePosition.GetWorld(index);
            if (position.isVisible == false)
                return Vector3.zero;

            return GetSmoothPosition(position.position.Inverse(), i);
        }

        private Vector3 GetSmoothPosition(Vector3 posePosition, int index)
        {
            var cachedPosition = _latestPositions[index];
            if (cachedPosition == Vector3.zero)
            {
                _latestPositions[index] = posePosition;
                return posePosition;
            }

            if (Vector3.Distance(posePosition, cachedPosition) <= smoothDistance)
                return cachedPosition;

            _latestPositions[index] = posePosition;
            return posePosition;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            foreach (Vector3 position in _gizmosPos)
                Gizmos.DrawSphere(position, gizmoSize);
        }
    }
}