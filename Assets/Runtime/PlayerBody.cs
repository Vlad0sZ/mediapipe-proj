using System.Collections.Generic;
using System.Linq;
using Mediapipe.Tasks.Components.Containers;
using UnityEngine;

namespace Runtime
{
    public class PlayerBody
    {
        private readonly int _playerIndex;

        private readonly List<Landmark> _worldLandmarks;
        private readonly List<NormalizedLandmark> _normalizedLandmarks;

        public PlayerBody(int index, Landmarks worldLandmarks, NormalizedLandmarks normalizedLandmarks)
        {
            _playerIndex = index;
            _worldLandmarks = worldLandmarks.landmarks.ToList();
            _normalizedLandmarks = normalizedLandmarks.landmarks.ToList();
        }

        private PlayerBody(int index, List<Landmark> worldLandmarks, List<NormalizedLandmark> normalizedLandmarks)
        {
            _playerIndex = index;
            _worldLandmarks = worldLandmarks;
            _normalizedLandmarks = normalizedLandmarks;
        }


        public VisibleJoint GetWorld(int joint) => 
            new VisibleJoint(_worldLandmarks[joint].ToVector(), _worldLandmarks[joint].visibility > 0.65f);

        public VisibleJoint GetNormalized(int joint) => 
            new VisibleJoint(_normalizedLandmarks[joint].ToVector(), _normalizedLandmarks[joint].visibility > 0.65f);

        public int GetIndex() => _playerIndex;
        public bool IsExists() => _worldLandmarks is {Count: > 0} && _normalizedLandmarks is {Count: > 0};

        public WorldPoseContainer ToWorldContainer() => new WorldPoseContainer(_worldLandmarks);

        public NormalizedPoseContainer ToNormalizedContainer() => new NormalizedPoseContainer(_normalizedLandmarks);


        public PlayerBody Clone() =>
            new(_playerIndex, _worldLandmarks.ToList(), _normalizedLandmarks.ToList());
        
        
        public struct VisibleJoint
        {
            public Vector3 position;
            public bool isVisible;

            public VisibleJoint(Vector3 position, bool isVisible)
            {
                this.position = position;
                this.isVisible = isVisible;
            }
        }
    }
}