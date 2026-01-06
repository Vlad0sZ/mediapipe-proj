using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SensorPack.KinectCore.Runtime.Views
{
    public class ColorView : MonoBehaviour
    {
        public RawImage view;
        private bool _needUpdate;

        private IEnumerator Start()
        {
            if (!view)
            {
                this.enabled = false;
                yield break;
            }

            yield return null;
            yield return new WaitUntil(KinectManager.IsKinectInitialized);
            yield return new WaitUntil(() => KinectManager.SensorAvailable);
            UpdateTexture();
        }

        private void Update()
        {
            if (_needUpdate)
                UpdateTexture();
        }

        private void UpdateTexture()
        {
            var scale = KinectManager.Instance.GetColorImageScale();
            view.texture = KinectManager.Instance.GetUsersClrTex2D();
            view.color = Color.white;
            view.uvRect = new Rect(0, 0, scale.x, scale.y);
            _needUpdate = view.texture == null;
        }
    }
}