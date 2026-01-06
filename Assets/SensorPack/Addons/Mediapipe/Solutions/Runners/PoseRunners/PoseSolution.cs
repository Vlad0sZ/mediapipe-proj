// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections;
using System.Text;
using Mediapipe;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe.Unity;
using Mediapipe.Unity.Experimental;
using UnityEngine;
using UnityEngine.Rendering;

namespace SensorPack.Addons.Mediapipe.Solutions.Runners.PoseRunners
{
    /// <summary>
    /// Класс для использования Mediapipe для детектирования ключевых точек скелетона
    /// </summary>
    public class PoseSolution : TaskRunner<PoseLandmarker>
    {
        private TextureFramePool _textureFramePool;

        public readonly PoseSolutionDetectionConfig Config = new PoseSolutionDetectionConfig();
        public event Action<PoseLandmarkerResult> OnResult;
        public bool FlipVertically { get; set; }
        public bool FlipHorizontally { get; set; }
        public Texture2D Texture => _latestTexture;

        private Texture2D _latestTexture;

        public override void Stop()
        {
            base.Stop();
            _textureFramePool?.Dispose();
            _textureFramePool = null;
            
            if(_latestTexture != null)
                UnityEngine.Object.Destroy(_latestTexture);

            _latestTexture = null;
        }

        protected override IEnumerator Run()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Delegate = {Config.Delegate}");
            sb.AppendLine($"Image Read Mode = {Config.ImageReadMode}");
            sb.AppendLine($"Model = {Config.ModelName}");
            sb.AppendLine($"Running Mode = {Config.RunningMode}");
            sb.AppendLine($"NumPoses = {Config.NumPoses}");
            sb.AppendLine($"MinPoseDetectionConfidence = {Config.MinPoseDetectionConfidence}");
            sb.AppendLine($"MinPosePresenceConfidence = {Config.MinPosePresenceConfidence}");
            sb.AppendLine($"MinTrackingConfidence = {Config.MinTrackingConfidence}");
            sb.AppendLine($"OutputSegmentationMasks = {Config.OutputSegmentationMasks}");
            Debug.Log(sb.ToString());

            AssetLoader.Provide(new StreamingAssetsResourceManager());

            yield return AssetLoader.PrepareAssetAsync(Config.ModelPath);

            var options = Config.GetPoseLandmarkerOptions(
                Config.RunningMode == RunningMode.LIVE_STREAM
                    ? OnPoseLandmarkDetectionOutput
                    : null);

            TaskApi = PoseLandmarker.CreateFromOptions(options, GpuManager.GpuResources);

            yield return ImageSource.Play();

            if (!ImageSource.isPrepared)
            {
                Debug.Log($"Failed to start ImageSource, exiting...");
                yield break;
            }

            // Use RGBA32 as the input format.
            // TODO: When using GpuBuffer, MediaPipe assumes that the input format is BGRA, so maybe the following code needs to be fixed.
            _textureFramePool = new TextureFramePool(ImageSource.textureWidth, ImageSource.textureHeight,
                TextureFormat.RGBA32, 10);

            _latestTexture = new Texture2D(ImageSource.textureWidth, ImageSource.textureHeight, TextureFormat.RGBA32,
                false)
            {
                name = "pipe-texture"
            };

            var transformationOptions = ImageSource.GetTransformationOptions();
            // FlipHorizontally = transformationOptions.flipHorizontally;
            // var flipVertically = transformationOptions.flipVertically;

            // Always setting rotationDegrees to 0 to avoid the issue that the detection becomes unstable when the input image is rotated.
            // https://github.com/homuler/MediaPipeUnityPlugin/issues/1196
            var imageProcessingOptions = new ImageProcessingOptions(rotationDegrees: 0);

            AsyncGPUReadbackRequest req = default;
            var waitForEndOfFrame = new WaitForEndOfFrame();
            var result = PoseLandmarkerResult.Alloc(options.numPoses, options.outputSegmentationMasks);

            // NOTE: we can share the GL context of the render thread with MediaPipe (for now, only on Android)
            var canUseGpuImage = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 &&
                                 GpuManager.GpuResources != null;
            using var glContext = canUseGpuImage ? GpuManager.GetGlContext() : null;

            while (true)
            {
                if (IsPaused)
                {
                    yield return new WaitWhile(() => IsPaused);
                }

                if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                // Build the input Image
                Image image;
                switch (Config.ImageReadMode)
                {
                    case ImageReadMode.GPU:
                        if (!canUseGpuImage)
                        {
                            throw new System.Exception("ImageReadMode.GPU is not supported");
                        }

                        image = GetGpuImage(textureFrame, FlipHorizontally, FlipVertically, glContext);
                        // TODO: Currently we wait here for one frame to make sure the texture is fully copied to the TextureFrame before sending it to MediaPipe.
                        // This usually works but is not guaranteed. Find a proper way to do this. See: https://github.com/homuler/MediaPipeUnityPlugin/pull/1311
                        yield return waitForEndOfFrame;
                        break;
                    case ImageReadMode.CPU:
                        yield return waitForEndOfFrame;
                        image = GetCpuImage(textureFrame, FlipHorizontally, FlipVertically);
                        break;
                    case ImageReadMode.CPUAsync:
                    default:
                        req = textureFrame.ReadTextureAsync(ImageSource.GetCurrentTexture(), FlipHorizontally,
                            FlipVertically);
                        yield return new WaitUntil(() => req.done);

                        if (req.hasError)
                        {
                            Debug.LogWarning($"Failed to read texture from the image source");
                            continue;
                        }

                        _latestTexture.LoadRawTextureData(textureFrame.GetRawTextureData<byte>());
                        _latestTexture.Apply();
                        image = textureFrame.BuildCPUImage();
                        textureFrame.Release();
                        break;
                }

                switch (TaskApi.runningMode)
                {
                    case RunningMode.IMAGE:
                        TaskApi.TryDetect(image, imageProcessingOptions, ref result);
                        DisposeAllMasks(result);
                        break;
                    case RunningMode.VIDEO:
                        TaskApi.TryDetectForVideo(image, GetCurrentTimestampMillisec(), imageProcessingOptions,
                            ref result);
                        DisposeAllMasks(result);
                        break;
                    case RunningMode.LIVE_STREAM:
                        TaskApi.DetectAsync(image, GetCurrentTimestampMillisec(), imageProcessingOptions);
                        break;
                }
            }
        }

        private Image GetCpuImage(TextureFrame textureFrame, bool flipHorizontally,
            bool flipVertically)
        {
            try
            {
                textureFrame.ReadTextureOnCPU(ImageSource.GetCurrentTexture(), flipHorizontally,
                    flipVertically);
                
                _latestTexture.LoadRawTextureData(textureFrame.GetRawTextureData<byte>());
                _latestTexture.Apply();
                return textureFrame.BuildCPUImage();
            }
            finally
            {
                textureFrame.Release();
            }
        }

        private void OnPoseLandmarkDetectionOutput(PoseLandmarkerResult result, Image image,
            long timestamp)
        {
            OnResult?.Invoke(result);
            DisposeAllMasks(result);
        }


        private Image GetGpuImage(
            TextureFrame textureFrame,
            bool flipHorizontally,
            bool flipVertically,
            GlContext glContext)
        {
            textureFrame.ReadTextureOnGPU(ImageSource.GetCurrentTexture(), flipHorizontally,
                flipVertically);

            Graphics.CopyTexture(textureFrame.texture, _latestTexture);
            _latestTexture.LoadRawTextureData(textureFrame.GetRawTextureData<byte>());
            _latestTexture.Apply();
            
            return textureFrame.BuildGPUImage(glContext);
        }

        private static void DisposeAllMasks(PoseLandmarkerResult result)
        {
            if (result.segmentationMasks != null)
            {
                foreach (var mask in result.segmentationMasks)
                {
                    mask.Dispose();
                }
            }
        }
    }
}