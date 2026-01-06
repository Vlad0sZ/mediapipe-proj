using SensorPack.KinectCore.Runtime.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace SensorPack.KinectCore.Runtime.Behaviours
{
    public class KinectBodyDrawer : MonoBehaviour
    {
        [SerializeField] private RawImage textureImage;

        private void OnEnable()
        {
            if (textureImage)
                textureImage.texture = null;
        }

        private void OnDisable()
        {
            if (textureImage)
                textureImage.texture = null;
        }

        private void Update()
        {
            if (textureImage == null || textureImage.texture != null)
                return;

            var backManager = BackgroundRemovalManager.Instance;
            var kinectManager = KinectManager.Instance;

            var userTex = backManager.GetForegroundTex();
            textureImage.texture = userTex;
            textureImage.rectTransform.localScale = kinectManager.GetColorImageScale();
            textureImage.color = Color.white;

            if (textureImage.TryGetComponent<AspectRatioFitter>(out var fitter))
                fitter.aspectRatio = userTex.width * 1f / userTex.height;
        }
    }
}