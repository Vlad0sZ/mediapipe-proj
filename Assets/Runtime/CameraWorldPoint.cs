using UnityEngine;

namespace Runtime
{
    public class CameraWorldPoint : MonoBehaviour
    {
        public Vector2 point;
        public Transform obj;

        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
        }


        private void Update()
        {
            var worldPos = _camera.ScreenToWorldPoint(point);
            Debug.DrawLine(point, worldPos, Color.red, 0.33f);
            if (obj)
                obj.position = worldPos;
        }
    }
}