using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using SensorPack.Addons.Mediapipe.ImageSource;
using SensorPack.Addons.Mediapipe.Solutions.Runners.PoseRunners;
using SensorPack.KinectCore.Runtime;
using SensorPack.KinectCore.Runtime.Interfaces;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SensorPack.Addons.Mediapipe
{
    /// <summary>
    /// Класс для работы с вебкамерой
    /// Использует MediaPipe для детектирования ключевых точек скелетона
    /// </summary>
    public sealed class WebcamInterface : DepthSensorInterface
    {
        private bool _backgroundRemovalInitialized;

        private const int DefaultColorWidth = 1280;
        private const int DefaultColorHeight = 720;
        private const int JointsCount = 25; // actual is 32.
        private const int BodyCount = 5; //

        private readonly object syncLock = new object();

        private ImageSource.ImageSource imageSource;
        private PoseSolution poseSolution;

        // Результат детекции позы
        private PoseLandmarkerResult _cachedResult;

        private KinectInterop.FrameSource _sensorFlags;
        private Color32[] colorImagePixels;

        // TODO #4?
        //ID должен быть не равен 0 для дальнейших проверок. Пока реализация работает для NumPoses = 1, поэтому defaultID = 1
        private int defaultID = 1;

        public bool FlipHorizontally
        {
            get => poseSolution.FlipHorizontally;
            set => poseSolution.FlipHorizontally = value;
        }


        /// <summary>
        /// Очищает ресурсы poseSolution и imageSource корректно
        /// </summary>
        private void CleanupResources()
        {
            if (poseSolution != null)
            {
                poseSolution.OnResult -= OnDetectResult;
                poseSolution.Stop();
                poseSolution = null;
            }

            if (imageSource != null)
            {
                imageSource.Stop();
                imageSource = null;
            }

            _cachedResult = default;
        }

        /// <summary>
        /// returns the depth sensor platform
        /// <returns>returns the depth sensor platform.</returns>
        /// </summary>
        public KinectInterop.DepthSensorPlatform GetSensorPlatform()
        {
            return KinectInterop.DepthSensorPlatform.Webcam;
        }

        /// <summary>
        /// initializes libraries and resources needed by this sensor interface
        /// <returns>returns true if the resources are successfully initialized, false otherwise.</returns>
        /// </summary>
        public bool InitSensorInterface(bool bCopyLibs, ref bool bNeedRestart)
        {
            bNeedRestart = false;
            
            var resolutions = new ImageSource.ImageSource.ResolutionStruct[]
            {
                new ImageSource.ImageSource.ResolutionStruct(DefaultColorWidth, DefaultColorHeight, 30),
            };

            imageSource = new WebCamSource(DefaultColorWidth, resolutions);
            
            return true;
            
            // No need to copy libs.
        }

        /// <summary>
        /// releases the resources and libraries used by this interface
        /// </summary>
        public void FreeSensorInterface(bool bDeleteLibs)
        {
            CleanupResources();
        }

        private void OnDetectResult(PoseLandmarkerResult result)
        {
            ProcessSkeleton(result);
        }

        private void ProcessSkeleton(PoseLandmarkerResult result)
        {
            lock (syncLock)
            {
                result.CloneTo(ref this._cachedResult);
            }
        }

        /// <summary>
        /// checks if there is available sensor on this interface
        /// returns true if there are available sensors on this interface, false otherwise
        /// </summary>
        public bool IsSensorAvailable()
        {
            bool bAvailable = GetSensorsCount() > 0;
            return bAvailable;
        }

        /// <summary>
        /// returns the number of available sensors, controlled by this interface
        /// </summary>
        public int GetSensorsCount()
        {
            return imageSource?.sourceCandidateNames.Length ?? 0;
        }

        /// <summary>
        /// opens the default sensor and inits needed resources. returns new sensor-data object
        /// </summary>
        public KinectInterop.SensorData OpenDefaultSensor(KinectInterop.FrameSource dwFlags, float sensorAngle,
            bool bUseMultiSource)
        {

            if (imageSource.sourceCandidateNames.Length == 0)
            {
                imageSource = null;
                return null;
            }

            poseSolution = Object.FindFirstObjectByType<PoseSolution>();
            if (poseSolution == null)
            {
                Debug.LogWarning($"No one object in scene with component {typeof(PoseSolution)}. Will be created.");
                poseSolution = new GameObject(typeof(PoseSolution).Namespace).AddComponent<PoseSolution>();
            }

            poseSolution.Initialize(imageSource);
            poseSolution.OnResult += OnDetectResult;
            
            KinectInterop.SensorData sensorData = new KinectInterop.SensorData();
            _sensorFlags = dwFlags;

            // TODO #3
            sensorData.bodyCount = 1;
            sensorData.jointCount = JointsCount;

            sensorData.depthCameraFOV = 60f;
            sensorData.colorCameraFOV = 53.8f;
            sensorData.depthCameraOffset = -0.05f;
            sensorData.faceOverlayOffset = -0.04f;

            imageSource.SelectSource(0);
            imageSource.SelectResolution(0);

            poseSolution.Play();

            float fWaitTime = Time.realtimeSinceStartup + 3f;
            while (!imageSource.isPrepared && Time.realtimeSinceStartup < fWaitTime)
            {
                // wait for sensor to be available
                Thread.Sleep(100);
            }

            // flip color image vertically
            sensorData.colorImageScale = new Vector3(imageSource.isHorizontallyFlipped ? -1f : 1f,
                imageSource.isVerticallyFlipped ? -1f : 1f, 1f);

            sensorData.colorImageTexture = imageSource.GetCurrentTexture();

            Debug.Log("Webcam-sensor " + (imageSource.isPrepared ? "prepared" : "not prepared"));

            return sensorData;
        }

        /// <summary>
        /// closes the sensor and frees used resources
        /// </summary>
        public void CloseSensor(KinectInterop.SensorData sensorData)
        {
            CleanupResources();
        }

        /// <summary>
        /// this method is invoked periodically to update sensor data, if needed
        /// returns true if update is successful, false otherwise
        /// </summary>
        public bool UpdateSensorData(KinectInterop.SensorData sensorData)
        {
            if (imageSource == null || !imageSource.isPrepared || !imageSource.isPlaying)
                return false;

            var texture = imageSource.GetCurrentTexture();

            if (sensorData.colorImageTexture == null)
                sensorData.colorImageTexture = texture;
            
            if (sensorData.colorImageTexture2D == null)
                sensorData.colorImageTexture2D =
                    new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);

            if (poseSolution.Texture != null)
                Graphics.CopyTexture(poseSolution.Texture, sensorData.colorImageTexture2D);

            //TODO
            return true;
        }

        /// <summary>
        /// gets next multi source frame, if one is available
        /// returns true if there is a new multi-source frame, false otherwise
        /// </summary>
        public bool GetMultiSourceFrame(KinectInterop.SensorData sensorData)
        {
            return false;
        }

        /// <summary>
        /// frees the resources taken by the last multi-source frame
        /// </summary>
        public void FreeMultiSourceFrame(KinectInterop.SensorData sensorData)
        {
        }

        /// <summary>
        /// polls for new body/skeleton frame. must fill in all needed body and joints' elements (tracking state and position)
        /// returns true if new body frame is available, false otherwise
        /// <returns>returns true if new body frame is available, false otherwise.</returns>
        /// </summary>
        public bool PollBodyFrame(KinectInterop.SensorData sensorData, ref KinectInterop.BodyFrameData bodyFrame,
            ref Matrix4x4 kinectToWorld, bool bIgnoreJointZ) //TODO
        {
            if (bodyFrame.bodyData == null || bodyFrame.bodyData.Length == 0)
                return false;

            if (_cachedResult.poseWorldLandmarks == null || _cachedResult.poseWorldLandmarks.Count == 0)
            {
                for (int i = 0; i < bodyFrame.bodyData.Length; i++)
                    bodyFrame.bodyData[i].bIsTracked = 0;

                // возвращаем true, так как данных либо нет, либо человек не распознается.
                return true;
            }

            PoseLandmarkerResult resultCur = new();
            lock (syncLock)
            {
                _cachedResult.CloneTo(ref resultCur);
            }

            JointMapper.MapJoints(sensorData, ref bodyFrame, defaultID, resultCur);
            return true;
        }

        /// <summary>
        /// polls for new color frame data
        /// returns true if new color frame is available, false otherwise
        /// <returns>returns true if new color frame is available, false otherwise.</returns>
        /// </summary>
        public bool PollColorFrame(KinectInterop.SensorData sensorData)
        {
            if (imageSource == null || !imageSource.isPrepared || !imageSource.isPlaying)
                return false;

            var texture = imageSource.GetCurrentTexture() as WebCamTexture;
            if (texture == null || !texture.didUpdateThisFrame)
                return false;

            if (sensorData.colorImage == null || colorImagePixels == null ||
                sensorData.colorImageWidth != texture.width || sensorData.colorImageHeight != texture.height)
            {
                if (texture.width > 0 && texture.height > 0)
                {
                    sensorData.colorImageWidth = texture.width;
                    sensorData.colorImageHeight = texture.height;
                    JointMapper.ColorWidth = texture.width;
                    JointMapper.ColorHeight = texture.height;

                    int framePixels = sensorData.colorImageWidth * sensorData.colorImageHeight;
                    sensorData.colorImage = new byte[framePixels * 4];
                    colorImagePixels = new Color32[framePixels];
                }
                else
                {
                    return false;
                }
            }


            texture.GetPixels32(colorImagePixels);
            var handle = GCHandle.Alloc(colorImagePixels, GCHandleType.Pinned);

            try
            {
                IntPtr srcPtr = handle.AddrOfPinnedObject();
                Marshal.Copy(srcPtr, sensorData.colorImage, 0, sensorData.colorImage.Length);

                sensorData.lastColorFrameTime = DateTime.Now.Ticks;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError("Error polling color frame: " + ex.Message);
                return false;
            }
            finally
            {
                handle.Free();
            }
        }

        public bool PollDepthFrame(KinectInterop.SensorData sensorData)
        {
            return false;
        }

        public bool PollInfraredFrame(KinectInterop.SensorData sensorData)
        {
            return false;
        }

        public void FixJointOrientations(KinectInterop.SensorData sensorData, ref KinectInterop.BodyData bodyData)
        {
            // from other sensors: no fixes are needed
        }

        public Vector2 MapSpacePointToDepthCoords(KinectInterop.SensorData sensorData, Vector3 spacePos)
        {
            // depth is not supported.
            return Vector2.zero;
        }

        public Vector3 MapDepthPointToSpaceCoords(KinectInterop.SensorData sensorData, Vector2 depthPos,
            ushort depthVal)
        {
            // depth is not supported.
            return Vector3.zero;
        }

        public bool MapDepthFrameToSpaceCoords(KinectInterop.SensorData sensorData, ref Vector3[] vSpaceCoords)
        {
            // depth is not supported.
            return false;
        }

        public Vector2 MapDepthPointToColorCoords(KinectInterop.SensorData sensorData, Vector2 depthPos,
            ushort depthVal)
        {
            // depth is not supported.
            return Vector2.zero;
        }

        public bool MapDepthFrameToColorCoords(KinectInterop.SensorData sensorData, ref Vector2[] vColorCoords)
        {
            // depth is not supported.
            return false;
        }

        public bool MapColorFrameToDepthCoords(KinectInterop.SensorData sensorData, ref Vector2[] vDepthCoords)
        {
            // depth is not supported.
            return false;
        }

        /// <summary>
        /// returns true if the face tracking is supported by this interface, false otherwise
        /// <returns> returns true if the face tracking is supported by this interface, false otherwise.</returns>
        /// </summary>
        public bool IsFaceTrackingAvailable(ref bool bNeedRestart)
        {
            bNeedRestart = false;
            return false;
        }

        public bool InitFaceTracking(bool bUseFaceModel, bool bDrawFaceRect)
        {
            return false;
        }

        public void FinishFaceTracking()
        {
        }

        public bool UpdateFaceTracking()
        {
            return true;
        }

        public bool IsFaceTrackingActive()
        {
            return false;
        }

        public bool IsDrawFaceRect()
        {
            return false;
        }

        public bool IsFaceTracked(long userId)
        {
            return false;
        }

        public bool GetFaceRect(long userId, ref UnityEngine.Rect faceRect)
        {
            return false;
        }

        public void VisualizeFaceTrackerOnColorTex(Texture2D texColor)
        {
        }

        public bool GetHeadPosition(long userId, ref Vector3 headPos)
        {
            return true;
        }

        public bool GetHeadRotation(long userId, ref Quaternion headRot)
        {
            return true;
        }

        public bool GetAnimUnits(long userId, ref Dictionary<KinectInterop.FaceShapeAnimations, float> afAU)
        {
            return true;
        }

        public bool GetShapeUnits(long userId, ref Dictionary<KinectInterop.FaceShapeDeformations, float> afSU)
        {
            return true;
        }

        public bool GetFaceProperties(long userId, ref Dictionary<string, string> faceProps)
        {
            return true;
        }

        public int GetFaceModelVerticesCount(long userId)
        {
            return 0;
        }

        public bool GetFaceModelVertices(long userId, ref Vector3[] avVertices)
        {
            return true;
        }

        public int GetFaceModelTrianglesCount()
        {
            return 0;
        }

        public bool GetFaceModelTriangles(bool bMirrored, ref int[] avTriangles)
        {
            return true;
        }

        public bool IsSpeechRecognitionAvailable(ref bool bNeedRestart)
        {
            return false;
        }

        public int InitSpeechRecognition(string sRecoCriteria, bool bUseKinect, bool bAdaptationOff)
        {
            return 0;
        }

        public void FinishSpeechRecognition()
        {
        }

        public int UpdateSpeechRecognition()
        {
            return 0;
        }

        public int LoadSpeechGrammar(string sFileName, short iLangCode, bool bDynamic)
        {
            return 0;
        }

        public int AddGrammarPhrase(string sFromRule, string sToRule, string sPhrase, bool bClearRulePhrases,
            bool bCommitGrammar)
        {
            return 0;
        }

        public void SetSpeechConfidence(float fConfidence)
        {
        }

        public bool IsSpeechStarted()
        {
            return false;
        }

        public bool IsSpeechEnded()
        {
            return false;
        }

        public bool IsPhraseRecognized()
        {
            return false;
        }

        public float GetPhraseConfidence()
        {
            return 0f;
        }

        public string GetRecognizedPhraseTag()
        {
            return string.Empty;
        }

        public void ClearRecognizedPhrase()
        {
        }

        public bool IsBackgroundRemovalAvailable(ref bool bNeedRestart)
        {
            _backgroundRemovalInitialized = KinectInterop.IsOpenCvAvailable(ref bNeedRestart);
            return _backgroundRemovalInitialized;
        }

        public bool InitBackgroundRemoval(KinectInterop.SensorData sensorData, bool isHiResPrefered)
        {
            return KinectInterop.InitBackgroundRemoval(sensorData, isHiResPrefered);
        }

        public void FinishBackgroundRemoval(KinectInterop.SensorData sensorData)
        {
            KinectInterop.FinishBackgroundRemoval(sensorData);
            _backgroundRemovalInitialized = false;
        }

        // this method gets invoked periodically to update the background removal
        // returns true if update is successful, false otherwise
        public bool UpdateBackgroundRemoval(KinectInterop.SensorData sensorData, bool isHiResPrefered,
            Color32 defaultColor, bool bAlphaTexOnly)
        {
            return KinectInterop.UpdateBackgroundRemoval(sensorData, isHiResPrefered, defaultColor, bAlphaTexOnly);
        }

        // returns true if background removal is initialized, false otherwise
        public bool IsBackgroundRemovalActive()
        {
            return _backgroundRemovalInitialized;
        }

        // returns true if BR-manager supports high resolution background removal
        public bool IsBRHiResSupported()
        {
            return true;
        }

        // returns the rectange of the foreground frame
        public UnityEngine.Rect GetForegroundFrameRect(KinectInterop.SensorData sensorData, bool isHiResPrefered)
        {
            return KinectInterop.GetForegroundFrameRect(sensorData, isHiResPrefered);
        }

        // returns the length of the foreground frame in bytes
        public int GetForegroundFrameLength(KinectInterop.SensorData sensorData, bool isHiResPrefered)
        {
            return KinectInterop.GetForegroundFrameLength(sensorData, isHiResPrefered);
        }

        // polls for new foreground frame data
        // returns true if foreground frame is available, false otherwise
        public bool PollForegroundFrame(KinectInterop.SensorData sensorData, bool isHiResPrefered, Color32 defaultColor,
            bool bLimitedUsers, ICollection<int> alTrackedIndexes, ref byte[] foregroundImage)
        {
            return KinectInterop.PollForegroundFrame(sensorData, isHiResPrefered, defaultColor, bLimitedUsers,
                alTrackedIndexes, ref foregroundImage);
        }

        //new function for checking any device about On/Off device
        public bool DeviceIsActive()
        {
            return IsSensorAvailable();
        }
    }
}