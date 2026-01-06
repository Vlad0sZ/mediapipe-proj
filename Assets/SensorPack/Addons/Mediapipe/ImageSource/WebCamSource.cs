// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mediapipe.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SensorPack.Addons.Mediapipe.ImageSource
{
    public class WebCamSource : ImageSource
    {
        private readonly int _preferableDefaultWidth;

        private readonly ResolutionStruct[] _defaultAvailableResolutions;

        public WebCamSource(int preferableDefaultWidth, ResolutionStruct[] defaultAvailableResolutions)
        {
            _preferableDefaultWidth = preferableDefaultWidth;
            _defaultAvailableResolutions = defaultAvailableResolutions;
        }

        private WebCamTexture _webCamTexture;

        private WebCamTexture WebCamTexture
        {
            get => _webCamTexture;
            set
            {
                if (_webCamTexture != null)
                    _webCamTexture.Stop();

                _webCamTexture = value;
            }
        }

        public override int textureWidth => !isPrepared ? 0 : WebCamTexture.width;
        public override int textureHeight => !isPrepared ? 0 : WebCamTexture.height;
        public override bool isVerticallyFlipped => isPrepared && WebCamTexture.videoVerticallyMirrored;
        public override bool isFrontFacing => isPrepared && (WebCamDevice?.isFrontFacing ?? false);

        public override RotationAngle rotation => !isPrepared
            ? RotationAngle.Rotation0
            : (RotationAngle) WebCamTexture.videoRotationAngle;

        private WebCamDevice? _webCamDevice;

        private WebCamDevice? WebCamDevice
        {
            get => _webCamDevice;
            set
            {
                if (_webCamDevice.HasValue && value.HasValue && value.Value.name == _webCamDevice.Value.name)
                {
                    // not changed
                    return;
                }


                if (value == null)
                {
                    // not changed
                    return;
                }

                _webCamDevice = value;
                resolution = GetDefaultResolution();
            }
        }

        public override string sourceName => WebCamDevice?.name;

        private WebCamDevice[] _availableSources;

        private WebCamDevice[] AvailableSources
        {
            get
            {
                if (_availableSources == null)
                {
                    _availableSources = WebCamTexture.devices;
                }

                return _availableSources;
            }
            set => _availableSources = value;
        }

        public override string[] sourceCandidateNames => AvailableSources?.Select(device => device.name).ToArray();

        public override ResolutionStruct[] availableResolutions
        {
            get { return WebCamDevice == null ? null : _defaultAvailableResolutions; }
        }

        public override bool isPrepared => WebCamTexture != null;
        public override bool isPlaying => WebCamTexture != null && WebCamTexture.isPlaying;

        private IEnumerator Initialize()
        {
            if (WebCamDevice != null)
            {
                yield break;
            }

            AvailableSources = WebCamTexture.devices;

            if (AvailableSources != null && AvailableSources.Length > 0)
            {
                WebCamDevice = AvailableSources[0];
            }
        }

        public override void SelectSource(int sourceId)
        {
            if (sourceId < 0 || sourceId >= AvailableSources.Length)
            {
                throw new ArgumentException($"Invalid source ID: {sourceId}");
            }

            WebCamDevice = AvailableSources[sourceId];
        }

        public override IEnumerator Play()
        {
            yield return Initialize();
            InitializeWebCamTexture();
            WebCamTexture.Play();
            yield return WaitForWebCamTexture();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public override IEnumerator Resume()
        {
            if (!isPrepared)
            {
                throw new InvalidOperationException("WebCamTexture is not prepared yet");
            }

            if (!WebCamTexture.isPlaying)
            {
                WebCamTexture.Play();
            }

            yield return WaitForWebCamTexture();
        }

        public override void Pause()
        {
            if (isPlaying)
            {
                WebCamTexture.Pause();
            }
        }

        public override void Stop()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (WebCamTexture != null)
            {
                WebCamTexture.Stop();
            }

            WebCamTexture = null;
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (isPrepared && !isPlaying)
                WebCamTexture.Play();
        }

        public override Texture GetCurrentTexture() => WebCamTexture;

        private ResolutionStruct GetDefaultResolution()
        {
            var resolutions = availableResolutions;
            return resolutions == null || resolutions.Length == 0
                ? new ResolutionStruct()
                : resolutions.OrderBy(res => res, new ResolutionStructComparer(_preferableDefaultWidth)).First();
        }

        private void InitializeWebCamTexture()
        {
            Stop();

            if (WebCamDevice.HasValue)
            {
                WebCamTexture = new WebCamTexture(WebCamDevice.Value.name, resolution.width, resolution.height,
                    (int) resolution.frameRate);
                return;
            }

            throw new InvalidOperationException("Cannot initialize WebCamTexture because WebCamDevice is not selected");
        }

        private IEnumerator WaitForWebCamTexture()
        {
            const int timeoutFrame = 2000;
            var count = 0;
            Debug.Log("Waiting for WebCamTexture to start");
            yield return new WaitUntil(() => count++ > timeoutFrame || WebCamTexture.width > 16);

            if (WebCamTexture.width <= 16)
            {
                throw new TimeoutException("Failed to start WebCam");
            }
        }

        private class ResolutionStructComparer : IComparer<ResolutionStruct>
        {
            private readonly int _preferableDefaultWidth;

            public ResolutionStructComparer(int preferableDefaultWidth)
            {
                _preferableDefaultWidth = preferableDefaultWidth;
            }

            public int Compare(ResolutionStruct a, ResolutionStruct b)
            {
                var aDiff = Mathf.Abs(a.width - _preferableDefaultWidth);
                var bDiff = Mathf.Abs(b.width - _preferableDefaultWidth);
                if (aDiff != bDiff)
                {
                    return aDiff - bDiff;
                }

                if (a.height != b.height)
                {
                    // prefer smaller height
                    return a.height - b.height;
                }

                // prefer smaller frame rate
                return (int) (a.frameRate - b.frameRate);
            }
        }
    }
}