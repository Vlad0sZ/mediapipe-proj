using System.Collections.Generic;
using System.Linq;
using R3;
using Runtime.Game.Publishers;
using UnityEngine;
using VContainer;

namespace Runtime
{
    public class AvatarController2 : MonoBehaviour
    {
        [System.Serializable]
        private class JointTransform
        {
            public JointType JointType;
            public Transform Joint;

            public Vector3 inverse = Vector3.one;
        }

        private readonly CompositeDisposable _subscription = new CompositeDisposable();
        private IPosePublisher _posePublisher;

        [SerializeField] private List<JointTransform> transforms;


        [Inject]
        public void Construct(IPosePublisher posePublisher)
        {
            _posePublisher = posePublisher;
        }

        private void Awake()
        {
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

            var container = firstPlayer.ToWorldContainer();
            foreach (var jointTransform in transforms)
            {
                var position = container[(int) jointTransform.JointType];
                position.z = 0;
                jointTransform.Joint.localPosition = Vector3.Scale(position, jointTransform.inverse);
            }
        }
    }
}