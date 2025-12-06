using System;
using System.Collections;
using Mediapipe;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe.Unity;
using Mediapipe.Unity.Experimental;
using Mediapipe.Unity.Sample;
using Mediapipe.Unity.Sample.PoseLandmarkDetection;
using R3;
using UnityEngine;

namespace Runtime
{
    public sealed class PoseLandmarkPublisher : VisionTaskApiRunner<PoseLandmarker>, IPoseLandmarkPublisher
    {
        public readonly PoseLandmarkDetectionConfig Config = new PoseLandmarkDetectionConfig();
        private readonly Subject<PoseLandmarkerResult> _onResult = new Subject<PoseLandmarkerResult>();
        public Observable<PoseLandmarkerResult> OnResult => _onResult;

        private TextureFramePool _textureFramePool;

        public override void Stop()
        {
            base.Stop();
            _textureFramePool?.Dispose();
            _textureFramePool = null;
        }


        protected override IEnumerator Run()
        {
            yield return AssetLoader.PrepareAssetAsync(Config.ModelPath);
            var options = CreateOptions();
            var imageSource = ImageSourceProvider.ImageSource;

            yield return imageSource.Play();

            if (!imageSource.isPrepared)
            {
                Mediapipe.Logger.LogError(TAG, "Failed to start ImageSource, exiting...");
                yield break;
            }

            _textureFramePool = new TextureFramePool(imageSource.textureWidth, imageSource.textureHeight,
                TextureFormat.RGBA32, 10);

            var transformationOptions = imageSource.GetTransformationOptions();
            var flipHorizontally = transformationOptions.flipHorizontally;
            var flipVertically = transformationOptions.flipVertically;

            var imageProcessingOptions = new ImageProcessingOptions(rotationDegrees: 0);

            UnityEngine.Rendering.AsyncGPUReadbackRequest req = default;
            var waitUntilReqDone = new WaitUntil(() => req.done);
            var result = PoseLandmarkerResult.Alloc(options.numPoses, options.outputSegmentationMasks);

            var canUseGpuImage = SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 &&
                                 GpuManager.GpuResources != null;
            using var glContext = canUseGpuImage ? GpuManager.GetGlContext() : null;

            while (true)
            {
                if (isPaused)
                {
                    yield return new WaitWhile(() => isPaused);
                }

                if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }


                Image image = null;
                switch (Config.ImageReadMode)
                {
                    case ImageReadMode.GPU:
                        if (!canUseGpuImage)
                            throw new Exception("ImageReadMode.GPU is not supported");

                        yield return GetImageOnGpu(glContext, textureFrame, imageSource, flipHorizontally,
                            flipVertically, i => image = i);
                        break;
                    case ImageReadMode.CPU:
                        yield return GetImageOnCpu(textureFrame, imageSource, flipHorizontally,
                            flipVertically, i => image = i);
                        break;
                    case ImageReadMode.CPUAsync:
                    default:
                        req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), flipHorizontally,
                            flipVertically);
                        yield return waitUntilReqDone;

                        if (req.hasError)
                        {
                            Debug.LogWarning($"Failed to read texture from the image source");
                            continue;
                        }

                        image = textureFrame.BuildCPUImage();
                        textureFrame.Release();
                        break;
                }

                switch (taskApi.runningMode)
                {
                    case Mediapipe.Tasks.Vision.Core.RunningMode.IMAGE:
                        DetectOnImage(image, imageProcessingOptions, ref result);
                        break;
                    case Mediapipe.Tasks.Vision.Core.RunningMode.VIDEO:
                        DetectOnVideo(image, imageProcessingOptions, ref result);
                        break;
                    case Mediapipe.Tasks.Vision.Core.RunningMode.LIVE_STREAM:
                    default:
                        DetectAsync(image, imageProcessingOptions);
                        break;
                }
            }
        }

        private PoseLandmarkerOptions CreateOptions()
        {
            var options = Config.GetPoseLandmarkerOptions(
                Config.RunningMode == Mediapipe.Tasks.Vision.Core.RunningMode.LIVE_STREAM
                    ? OnPoseLandmarkDetectionOutput
                    : null);

            taskApi = PoseLandmarker.CreateFromOptions(options, GpuManager.GpuResources);
            return options;
        }

        private static IEnumerator GetImageOnGpu(GlContext glContext, TextureFrame textureFrame,
            ImageSource imageSource,
            bool flipH,
            bool flipV, Action<Image> callback)
        {
            textureFrame.ReadTextureOnGPU(imageSource.GetCurrentTexture(), flipH, flipV);
            var image = textureFrame.BuildGPUImage(glContext);

            yield return new WaitForEndOfFrame();
            callback?.Invoke(image);
        }

        private static IEnumerator GetImageOnCpu(TextureFrame textureFrame, ImageSource imageSource, bool flipH,
            bool flipV,
            Action<Image> callback)
        {
            yield return new WaitForEndOfFrame();
            textureFrame.ReadTextureOnCPU(imageSource.GetCurrentTexture(), flipH, flipV);
            var image = textureFrame.BuildCPUImage();
            textureFrame.Release();
            callback?.Invoke(image);
        }


        private void DetectOnImage(Image image, ImageProcessingOptions options, ref PoseLandmarkerResult result)
        {
            bool detected = taskApi.TryDetect(image, options, ref result);
            _onResult.OnNext(detected ? result : default);
            DisposeAllMasks(result);
        }

        private void DetectOnVideo(Image image, ImageProcessingOptions options, ref PoseLandmarkerResult result)
        {
            bool detected = taskApi.TryDetectForVideo(image, GetCurrentTimestampMillisec(), options, ref result);
            _onResult.OnNext(detected ? result : default);
            DisposeAllMasks(result);
        }

        private void DetectAsync(Image image, ImageProcessingOptions options) =>
            taskApi.DetectAsync(image, GetCurrentTimestampMillisec(), options);


        private void OnPoseLandmarkDetectionOutput(PoseLandmarkerResult result, Image image, long timestamp)
        {
            _onResult.OnNext(result);
            DisposeAllMasks(result);
        }

        private static void DisposeAllMasks(PoseLandmarkerResult result)
        {
            if (result.segmentationMasks == null)
                return;

            foreach (var mask in result.segmentationMasks)
                mask.Dispose();
        }
    }
}