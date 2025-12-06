using System.Collections.Generic;
using System.Linq;
using R3;
using Runtime.Game.Publishers;
using UnityEngine;
using VContainer;

namespace Runtime
{
    public class AvatarController : MonoBehaviour
    {
        [System.Serializable]
        public class ArmBone
        {
            public Transform target;
            public Transform hint;

            public Transform shoulderBone;
            public Transform elbowBone;
            public Transform wristBone;

            public Vector3 rot;

            public float upperArmLen;
            public float forearmLen;


            public void CalculateLength()
            {
                var elbow = elbowBone.position;
                upperArmLen = Vector3.Distance(shoulderBone.position, elbow);
                forearmLen = Vector3.Distance(elbow, wristBone.position);
            }
        }

        [SerializeField] private Transform body;
        [SerializeField] private ArmBone[] armBones;
        [SerializeField] private float size;
        private readonly int[] leftArmIdx = {11, 13, 15, 19};
        private readonly int[] rightArmIdx = {12, 14, 16, 20};
        private readonly CompositeDisposable _subscription = new CompositeDisposable();

        private IPosePublisher _posePublisher;
        private Camera _camera;
        private List<PlayerBody> _latestBodies;

        [Inject]
        public void Construct(IPosePublisher posePublisher)
        {
            _posePublisher = posePublisher;
        }

        private void Awake()
        {
            foreach (var bone in armBones)
                bone.CalculateLength();

            _posePublisher.Bodies.Subscribe(OnPose)
                .AddTo(_subscription);

            _camera = Camera.main;
        }

        private void OnDestroy() =>
            _subscription.Dispose();

        private void OnPose(List<PlayerBody> obj)
        {
            var firstPlayer = obj.FirstOrDefault(x => x != null && x.IsExists());
            _latestBodies = obj.ToList();
            if (firstPlayer == null)
                return;

            var container = firstPlayer.ToWorldContainer();
            ApplyArm(container, armBones[0], leftArmIdx);
            ApplyArm(container, armBones[1], rightArmIdx);
            ApplyBody(firstPlayer.ToNormalizedContainer());
        }

        private void ApplyBody<T>(Container<T, Vector3> container)
        {
            var center = container[0];
            var worldPos = _camera.ViewportToWorldPoint(center);
            var localPos = body.localPosition;
            localPos.x = worldPos.x;
            body.localPosition = localPos;
        }


        private void ApplyArm<T>(Container<T, Vector3> pose, ArmBone armBone, int[] arms)
        {
            var shoulderPos = pose[arms[0]];
            var elbowPos = pose[arms[1]];
            var wristPos = pose[arms[2]];

            var skeletonLength = armBones[0].forearmLen;
            var humanLength = Vector3.Distance(shoulderPos, elbowPos);
            var diff = skeletonLength / humanLength;

            var shoulderBone = armBone.elbowBone;
            var upperArmDir = (elbowPos - shoulderPos) * diff;
            var forearmDir = (wristPos - elbowPos) * diff;

            var elbowTarget = shoulderBone.position + upperArmDir;
            var wristTarget = elbowTarget + forearmDir;

            armBone.hint.position = elbowTarget;
            armBone.target.position = wristTarget;
            armBone.target.rotation =
                Quaternion.FromToRotation(upperArmDir, forearmDir) * Quaternion.Euler(armBone.rot);
        }

        private void OnDrawGizmos()
        {
            if (_latestBodies == null)
                return;

            foreach (var playerBody in _latestBodies)
            {
                if (playerBody == null)
                    continue;

                var pose = playerBody.ToWorldContainer();
                var shoulder = pose[leftArmIdx[0]];
                var elbow = pose[leftArmIdx[1]];
                var wrist = pose[leftArmIdx[2]];

                var skeletonLength = armBones[0].forearmLen;
                var humanLength = Vector3.Distance(shoulder, elbow);


                #if UNITY_EDITOR
                UnityEditor.Handles.Label(shoulder, $@"hl: {humanLength}");
                #endif
                var diff = skeletonLength / humanLength;
                var upperArm = (elbow - shoulder) * diff;
                var forearm = (wrist - elbow) * diff;

                var shoulderBone = armBones[0].elbowBone.position;
                var targetPos = shoulderBone + upperArm;
                var targetPos2 = targetPos + forearm;

                Gizmos.color = Color.green;
                Gizmos.DrawSphere(shoulderBone, size);
                Gizmos.DrawSphere(targetPos, size);
                Gizmos.DrawSphere(targetPos2, size);
                Gizmos.DrawLine(shoulderBone, targetPos);
                Gizmos.DrawLine(targetPos, targetPos2);


                Gizmos.color = Color.red;

                Gizmos.DrawLine(shoulder, elbow);
                Gizmos.DrawLine(elbow, wrist);
                Gizmos.DrawSphere(shoulder, size);
                Gizmos.DrawSphere(elbow, size);
                Gizmos.DrawSphere(wrist, size);
            }
        }
    }
}