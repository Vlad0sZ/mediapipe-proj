using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Runtime.Game.Publishers;
using UnityEngine;
using VContainer;

namespace Runtime
{
    public class AvatarPositionController : MonoBehaviour
    {
        [SerializeField] private Transform body;
        private readonly CompositeDisposable _subscription = new CompositeDisposable();
        private IPosePublisher _posePublisher;
        private Camera _camera;

        [Inject]
        public void Construct(IPosePublisher posePublisher)
        {
            _posePublisher = posePublisher;
        }

        private void Awake()
        {
            _posePublisher.Bodies.Subscribe(OnPose)
                .AddTo(_subscription);

            _camera = Camera.main;
        }

        private void OnDestroy() =>
            _subscription.Dispose();

        private void OnPose(List<PlayerBody> obj)
        {
            var firstPlayer = obj.FirstOrDefault(x => x != null && x.IsExists());
            if (firstPlayer == null)
                return;

            ApplyBody(firstPlayer.ToNormalizedContainer());
        }

        private void ApplyBody<T>(Container<T, Vector3> container)
        {
            var center = container[0];
            var worldPos = center.ToWorld(_camera, 1f);
            Debug.DrawLine(center, worldPos, Color.red, 1f);
            
            var localPos = body.position;
            localPos.x = worldPos.x;
            body.position = localPos;
        }
    }
}