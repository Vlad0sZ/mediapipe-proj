using System.Collections;
using System.Linq;
using Mediapipe;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe.Unity;
using Mediapipe.Unity.Experimental;
using Mediapipe.Unity.Sample;
using Mediapipe.Unity.Sample.Holistic;
using R3;
using UnityEngine;
using UnityEngine.Rendering;
using Detection = Mediapipe.Detection;

namespace Runtime
{
    public class HolisticLandmarkPublisher : LegacySolutionRunner<HolisticTrackingGraph>, IPoseLandmarkPublisher
    {
        private TextureFramePool _textureFramePool;
        private PoseLandmarkerResult _allocatedResult;

        private Subject<PoseLandmarkerResult> _subjectResult = new Subject<PoseLandmarkerResult>();

        private readonly object _lockObj = new();
        public R3.Observable<PoseLandmarkerResult> OnResult => _subjectResult;


        public HolisticTrackingGraph.ModelComplexity modelComplexity
        {
            get => graphRunner.modelComplexity;
            set => graphRunner.modelComplexity = value;
        }

        public bool smoothLandmarks
        {
            get => graphRunner.smoothLandmarks;
            set => graphRunner.smoothLandmarks = value;
        }

        public bool refineFaceLandmarks
        {
            get => graphRunner.refineFaceLandmarks;
            set => graphRunner.refineFaceLandmarks = value;
        }

        public bool enableSegmentation
        {
            get => graphRunner.enableSegmentation;
            set => graphRunner.enableSegmentation = value;
        }

        public bool smoothSegmentation
        {
            get => graphRunner.smoothSegmentation;
            set => graphRunner.smoothSegmentation = value;
        }

        public float minDetectionConfidence
        {
            get => graphRunner.minDetectionConfidence;
            set => graphRunner.minDetectionConfidence = value;
        }

        public float minTrackingConfidence
        {
            get => graphRunner.minTrackingConfidence;
            set => graphRunner.minTrackingConfidence = value;
        }

        public override void Stop()
        {
            base.Stop();
            _textureFramePool?.Dispose();
            _textureFramePool = null;
        }

        protected override IEnumerator Run()
        {
            var graphInitRequest = graphRunner.WaitForInit(runningMode);
            var imageSource = ImageSourceProvider.ImageSource;

            yield return imageSource.Play();

            if (!imageSource.isPrepared)
            {
                UnityEngine.Debug.LogError("Failed to start ImageSource, exiting...");
                yield break;
            }

            // Use RGBA32 as the input format.
            // TODO: When using GpuBuffer, MediaPipe assumes that the input format is BGRA, so the following code must be fixed.
            _textureFramePool = new TextureFramePool(imageSource.textureWidth, imageSource.textureHeight,
                TextureFormat.RGBA32, 10);

            _allocatedResult = PoseLandmarkerResult.Alloc(1, enableSegmentation);

            // NOTE: The screen will be resized later, keeping the aspect ratio.
            if (screen)
                screen.Initialize(imageSource);

            yield return graphInitRequest;
            if (graphInitRequest.isError)
            {
                Debug.LogError(graphInitRequest.error);
                yield break;
            }

            if (!runningMode.IsSynchronous())
            {
                graphRunner.OnPoseDetectionOutput += OnPoseDetectionOutput;
                graphRunner.OnFaceLandmarksOutput += OnFaceLandmarksOutput;
                graphRunner.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
                graphRunner.OnLeftHandLandmarksOutput += OnLeftHandLandmarksOutput;
                graphRunner.OnRightHandLandmarksOutput += OnRightHandLandmarksOutput;
                graphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;
                graphRunner.OnSegmentationMaskOutput += OnSegmentationMaskOutput;
                graphRunner.OnPoseRoiOutput += OnPoseRoiOutput;
            }

            graphRunner.StartRun(imageSource);

            AsyncGPUReadbackRequest req = default;
            var waitUntilReqDone = new WaitUntil(() => req.done);

            // NOTE: we can share the GL context of the render thread with MediaPipe (for now, only on Android)
            var canUseGpuImage = graphRunner.configType == GraphRunner.ConfigType.OpenGLES &&
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

                // Copy current image to TextureFrame
                if (canUseGpuImage)
                {
                    yield return new WaitForEndOfFrame();
                    textureFrame.ReadTextureOnGPU(imageSource.GetCurrentTexture());
                }
                else
                {
                    req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), false,
                        imageSource.isVerticallyFlipped);
                    yield return waitUntilReqDone;

                    if (req.hasError)
                    {
                        Debug.LogWarning($"Failed to read texture from the image source");
                        yield return new WaitForEndOfFrame();
                        continue;
                    }
                }

                graphRunner.AddTextureFrameToInputStream(textureFrame, glContext);

                if (runningMode.IsSynchronous())
                {
                    screen.ReadSync(textureFrame);

                    var task = graphRunner.WaitNextAsync();
                    yield return new WaitUntil(() => task.IsCompleted);

                    var result = task.Result;

                    _allocatedResult.poseWorldLandmarks.Clear();
                    _allocatedResult.poseLandmarks.Clear();

                    var worldLandmarks = Landmarks.CreateFrom(result.poseWorldLandmarks);
                    _allocatedResult.poseWorldLandmarks.Add(worldLandmarks);

                    var normalizedLandmarks = NormalizedLandmarks.CreateFrom(result.poseLandmarks);
                    _allocatedResult.poseLandmarks.Add(normalizedLandmarks);

                    _subjectResult.OnNext(_allocatedResult);

                    result.segmentationMask?.Dispose();
                }
            }
        }

        private void OnPoseDetectionOutput(object stream, OutputStream<Detection>.OutputEventArgs eventArgs)
        {
            var packet = eventArgs.packet;
            var value = packet?.Get(Detection.Parser);
        }

        private void OnFaceLandmarksOutput(object stream,
            OutputStream<NormalizedLandmarkList>.OutputEventArgs eventArgs)
        {
            var packet = eventArgs.packet;
            var value = packet?.Get(NormalizedLandmarkList.Parser);
        }

        private void OnPoseLandmarksOutput(object stream,
            OutputStream<NormalizedLandmarkList>.OutputEventArgs eventArgs)
        {
            var packet = eventArgs.packet;
            var value = packet?.Get(NormalizedLandmarkList.Parser);

            if (value?.Landmark == null || value?.Landmark.Count == 0)
                return;

            lock (_lockObj)
            {
                var landmarks = NormalizedLandmarks.CreateFrom(value);
                _allocatedResult.poseLandmarks.Clear();
                _allocatedResult.poseLandmarks.Add(landmarks);
            }
        }

        private void OnLeftHandLandmarksOutput(object stream,
            OutputStream<NormalizedLandmarkList>.OutputEventArgs eventArgs)
        {
            var packet = eventArgs.packet;
            var value = packet?.Get(NormalizedLandmarkList.Parser);
        }

        private void OnRightHandLandmarksOutput(object stream,
            OutputStream<NormalizedLandmarkList>.OutputEventArgs eventArgs)
        {
            var packet = eventArgs.packet;
            var value = packet?.Get(NormalizedLandmarkList.Parser);
        }

        private void OnPoseWorldLandmarksOutput(object stream, OutputStream<LandmarkList>.OutputEventArgs eventArgs)
        {
            var packet = eventArgs.packet;
            var value = packet?.Get(LandmarkList.Parser);

            if (value?.Landmark == null || value?.Landmark.Count == 0)
                return;

            lock (_lockObj)
            {
                var landmarks = Landmarks.CreateFrom(value);
                _allocatedResult.poseWorldLandmarks.Clear();
                _allocatedResult.poseWorldLandmarks.Add(landmarks);
            }
        }

        private void OnSegmentationMaskOutput(object stream, OutputStream<ImageFrame>.OutputEventArgs eventArgs)
        {
            var packet = eventArgs.packet;
            var value = packet?.Get();

            //

            value?.Dispose();
        }

        private void OnPoseRoiOutput(object stream, OutputStream<NormalizedRect>.OutputEventArgs eventArgs)
        {
            var packet = eventArgs.packet;
            var value = packet?.Get(NormalizedRect.Parser);

            lock (_lockObj)
            {
                _subjectResult.OnNext(_allocatedResult);
            }
        }
    }
}