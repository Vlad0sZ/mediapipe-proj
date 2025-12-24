using System;
using System.Collections.Generic;
using System.Linq;
using Mediapipe.Unity.Sample;
using Runtime.Infrastructure.Video;
using TMPro;
using UnityEngine;
using VContainer;

namespace Runtime.UI
{
    public class SettingsScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;

        private IWebCamInitializer _webCamInitializer;
        private IImageSourceProvider _imageSourceProvider;

        [Inject]
        public void Construct(IWebCamInitializer webCamInitializer, IImageSourceProvider imageSourceProvider)
        {
            _webCamInitializer = webCamInitializer;
            _imageSourceProvider = imageSourceProvider;
        }

        private void OnEnable() => dropdown.onValueChanged.AddListener(OnCameraSelected);

        private void OnDisable() => dropdown.onValueChanged.RemoveListener(OnCameraSelected);

        private void Start() => BindWebCams();

        public void BindWebCams()
        {
            var devices = _webCamInitializer
                .GetDevices()
                .Select(x => new TMP_Dropdown.OptionData(x.name))
                .ToList();

            dropdown.ClearOptions();
            dropdown.AddOptions(devices.ToList());

            if (ImageSourceProvider.CurrentSourceType == ImageSourceType.WebCamera)
            {
                var currentImageSource = ImageSourceProvider.ImageSource;
                var index = dropdown.options.FindIndex(x => x.text.Equals(currentImageSource.sourceName));
                dropdown.SetValueWithoutNotify(index >= 0 ? index : 0);
            }
            else
            {
                dropdown.SetValueWithoutNotify(0);
            }
        }

        private void OnCameraSelected(int cameraIndex) =>
            _imageSourceProvider.ChangeSource(cameraIndex);
    }
}