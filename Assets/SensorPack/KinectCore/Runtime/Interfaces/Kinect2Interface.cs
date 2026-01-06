#if (UNITY_STANDALONE_WIN)
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using Windows.Kinect;
using Microsoft.Kinect.Face;
using UnityEngine;

// Класс поменял, убрал ненужные вызовы =null
// Для передачи данных из native-lib использовал Marshal + глобальные IntPtr-указатели, которые создаются в момент открытия (OpenDefaultSensor)
// и очищаются в момент закрытия (CloseSensor)
// Также пропатчил Kinect2UnityWrapper - где сделал видимым internal для Kinect.InternalAPIBridge и от Kinect.InternalAPIBridge001 до ...005
namespace SensorPack.KinectCore.Runtime.Interfaces
{
    public sealed class Kinect2Interface : DepthSensorInterface
    {
        #region Imported DLL extern

        // DLL Imports for speech wrapper functions
        [DllImport("Kinect2SpeechWrapper", EntryPoint = "InitSpeechRecognizer")]
        private static extern int InitSpeechRecognizerNative([MarshalAs(UnmanagedType.LPWStr)] string sRecoCriteria,
            bool bUseKinect, bool bAdaptationOff);

        [DllImport("Kinect2SpeechWrapper", EntryPoint = "FinishSpeechRecognizer")]
        private static extern void FinishSpeechRecognizerNative();

        [DllImport("Kinect2SpeechWrapper", EntryPoint = "UpdateSpeechRecognizer")]
        private static extern int UpdateSpeechRecognizerNative();

        [DllImport("Kinect2SpeechWrapper", EntryPoint = "LoadSpeechGrammar")]
        private static extern int LoadSpeechGrammarNative([MarshalAs(UnmanagedType.LPWStr)] string sFileName,
            short iNewLangCode, bool bDynamic);

        [DllImport("Kinect2SpeechWrapper", EntryPoint = "AddSpeechGrammar")]
        private static extern int AddSpeechGrammarNative([MarshalAs(UnmanagedType.LPWStr)] string sFileName,
            short iNewLangCode, bool bDynamic);

        [DllImport("Kinect2SpeechWrapper", EntryPoint = "AddGrammarPhrase")]
        private static extern int AddGrammarPhraseNative([MarshalAs(UnmanagedType.LPWStr)] string sFromRule,
            [MarshalAs(UnmanagedType.LPWStr)] string sToRule, [MarshalAs(UnmanagedType.LPWStr)] string sPhrase,
            bool bClearRule, bool bCommitGrammar);

        [DllImport("Kinect2SpeechWrapper", EntryPoint = "AddPhraseToGrammar")]
        private static extern int AddPhraseToGrammarNative([MarshalAs(UnmanagedType.LPWStr)] string sGrammarName,
            [MarshalAs(UnmanagedType.LPWStr)] string sFromRule, [MarshalAs(UnmanagedType.LPWStr)] string sToRule,
            [MarshalAs(UnmanagedType.LPWStr)] string sPhrase, bool bClearRule, bool bCommitGrammar);

        [DllImport("Kinect2SpeechWrapper", EntryPoint = "SetGrammarState")]
        private static extern int SetGrammarStateNative([MarshalAs(UnmanagedType.LPWStr)] string sGrammarName,
            bool bEnableGrammar);

        [DllImport("Kinect2SpeechWrapper", EntryPoint = "SetRequiredConfidence")]
        private static extern void SetSpeechConfidenceNative(float fConfidence);

        [DllImport("Kinect2SpeechWrapper", EntryPoint = "IsSoundStarted")]
        private static extern bool IsSpeechStartedNative();

        [DllImport("Kinect2SpeechWrapper", EntryPoint = "IsSoundEnded")]
        private static extern bool IsSpeechEndedNative();

        [DllImport("Kinect2SpeechWrapper", EntryPoint = "IsPhraseRecognized")]
        private static extern bool IsPhraseRecognizedNative();

        [DllImport("Kinect2SpeechWrapper", EntryPoint = "GetPhraseConfidence")]
        private static extern float GetPhraseConfidenceNative();

        [DllImport("Kinect2SpeechWrapper", EntryPoint = "GetRecognizedTag")]
        private static extern IntPtr GetRecognizedPhraseTagNative();

        [DllImport("Kinect2SpeechWrapper", EntryPoint = "ClearPhraseRecognized")]
        private static extern void ClearRecognizedPhraseNative();

        #endregion

        // измените это значение на false, если используете не только Kinect-v2 и хотите, чтобы KM проверял наличие доступных датчиков
        private const bool SensorAlwaysAvailable = false;

        public KinectSensor KinectSensor;

        private KinectInterop.FrameSource _sensorFlags;
        private CoordinateMapper _coordinateMapper;

        // Reader-потоков кинекта
        private BodyFrameReader _bodyFrameReader;
        private BodyIndexFrameReader _bodyIndexFrameReader;
        private ColorFrameReader _colorFrameReader;
        private DepthFrameReader _depthFrameReader;
        private InfraredFrameReader _infraredFrameReader;

        // MultiReader, если установлена галочка useMultiSource в KM
        private MultiSourceFrameReader _multiSourceFrameReader;

        // Frame-потоков кинекта
        private MultiSourceFrame _multiSourceFrame;

        private BodyFrame _msBodyFrame;
        private BodyIndexFrame _msBodyIndexFrame;
        private ColorFrame _msColorFrame;
        private DepthFrame _msDepthFrame;
        private InfraredFrame _msInfraredFrame;

        // Данные о человеках
        private int _bodyCount;
        private Body[] _bodyData;

        private bool _bFaceTrackingInit;
        public FaceFrameSource[] FaceFrameSources;
        public FaceFrameReader[] FaceFrameReaders;
        public FaceFrameResult[] FaceFrameResults;

        private bool _isDrawFaceRect;
        public HighDefinitionFaceFrameSource[] HdFaceFrameSources;
        public HighDefinitionFaceFrameReader[] HdFaceFrameReaders;
        public FaceAlignment[] HdFaceAlignments;
        public FaceModel[] HdFaceModels;

        private bool _bBackgroundRemovalInit;


        // Переменные для чтения потоков (сюда копируются данные из native-lib)
        private IntPtr _persistentColorBufferPtr = IntPtr.Zero;
        private IntPtr _persistentDepthBufferPtr = IntPtr.Zero;
        private IntPtr _persistentBodyIndexBufferPtr = IntPtr.Zero;
        private IntPtr _persistentInfraredBufferPtr = IntPtr.Zero;

        // Временные массивы байтов (буффер), их копируем в ushort[] (тк Marshal.Copy не умеет копирование ushort)
        private byte[] _marshalTempDepth;
        private byte[] _marshalTempInfrared;

        public KinectInterop.DepthSensorPlatform GetSensorPlatform() => 
            KinectInterop.DepthSensorPlatform.KinectSDKv2;

        public bool InitSensorInterface(bool bCopyLibs, ref bool bNeedRestart)
        {
            bool bOneCopied = false, bAllCopied = true;
            string sTargetPath = KinectInterop.GetTargetDllPath(".", KinectInterop.Is64bitArchitecture()) + "/";

            if (!bCopyLibs)
            {
                // check if the native library is there
                string sTargetLib = sTargetPath + "KinectUnityAddin.dll";
                bNeedRestart = false;

                string sZipFileName = !KinectInterop.Is64bitArchitecture()
                    ? "KinectV2UnityAddin.x86.zip"
                    : "KinectV2UnityAddin.x64.zip";
                long iTargetSize = KinectInterop.GetUnzippedEntrySize(sZipFileName, "KinectUnityAddin.dll");
            
                return KinectInterop.IsFileExists(sTargetLib, iTargetSize);
            }

            if (!KinectInterop.Is64bitArchitecture())
            {
                Dictionary<string, string> dictFilesToUnzip = new Dictionary<string, string>();
                dictFilesToUnzip["KinectUnityAddin.dll"] = sTargetPath + "KinectUnityAddin.dll";
                dictFilesToUnzip["Kinect20.Face.dll"] = sTargetPath + "Kinect20.Face.dll";
                dictFilesToUnzip["KinectFaceUnityAddin.dll"] = sTargetPath + "KinectFaceUnityAddin.dll";
                dictFilesToUnzip["Kinect2SpeechWrapper.dll"] = sTargetPath + "Kinect2SpeechWrapper.dll";
                dictFilesToUnzip["Kinect20.VisualGestureBuilder.dll"] = sTargetPath + "Kinect20.VisualGestureBuilder.dll";
                dictFilesToUnzip["KinectVisualGestureBuilderUnityAddin.dll"] =
                    sTargetPath + "KinectVisualGestureBuilderUnityAddin.dll";
                dictFilesToUnzip["vgbtechs/AdaBoostTech.dll"] = sTargetPath + "vgbtechs/AdaBoostTech.dll";
                dictFilesToUnzip["vgbtechs/RFRProgressTech.dll"] = sTargetPath + "vgbtechs/RFRProgressTech.dll";
                dictFilesToUnzip["msvcp110.dll"] = sTargetPath + "msvcp110.dll";
                dictFilesToUnzip["msvcr110.dll"] = sTargetPath + "msvcr110.dll";

                KinectInterop.UnzipResourceFiles(dictFilesToUnzip, "KinectV2UnityAddin.x86.zip", ref bOneCopied,
                    ref bAllCopied);
            }
            else
            {
                Dictionary<string, string> dictFilesToUnzip = new Dictionary<string, string>();
                dictFilesToUnzip["KinectUnityAddin.dll"] = sTargetPath + "KinectUnityAddin.dll";
                dictFilesToUnzip["Kinect20.Face.dll"] = sTargetPath + "Kinect20.Face.dll";
                dictFilesToUnzip["KinectFaceUnityAddin.dll"] = sTargetPath + "KinectFaceUnityAddin.dll";
                dictFilesToUnzip["Kinect2SpeechWrapper.dll"] = sTargetPath + "Kinect2SpeechWrapper.dll";
                dictFilesToUnzip["Kinect20.VisualGestureBuilder.dll"] = sTargetPath + "Kinect20.VisualGestureBuilder.dll";
                dictFilesToUnzip["KinectVisualGestureBuilderUnityAddin.dll"] =
                    sTargetPath + "KinectVisualGestureBuilderUnityAddin.dll";
                dictFilesToUnzip["vgbtechs/AdaBoostTech.dll"] = sTargetPath + "vgbtechs/AdaBoostTech.dll";
                dictFilesToUnzip["vgbtechs/RFRProgressTech.dll"] = sTargetPath + "vgbtechs/RFRProgressTech.dll";
                dictFilesToUnzip["msvcp110.dll"] = sTargetPath + "msvcp110.dll";
                dictFilesToUnzip["msvcr110.dll"] = sTargetPath + "msvcr110.dll";

                KinectInterop.UnzipResourceFiles(dictFilesToUnzip, "KinectV2UnityAddin.x64.zip", ref bOneCopied,
                    ref bAllCopied);
            }

            KinectInterop.UnzipResourceDirectory(sTargetPath, "NuiDatabase.zip", sTargetPath + "NuiDatabase");

            bNeedRestart = (bOneCopied && bAllCopied);

            return true;
        }

        public void FreeSensorInterface(bool bDeleteLibs)
        {
            if (!bDeleteLibs)
                return;
        
            KinectInterop.DeleteNativeLib("KinectUnityAddin.dll", true);
            KinectInterop.DeleteNativeLib("msvcp110.dll", false);
            KinectInterop.DeleteNativeLib("msvcr110.dll", false);
        }

        public bool IsSensorAvailable()
        {
            KinectSensor sensor = KinectSensor.GetDefault();

            if (sensor == null) 
                return false;
        
            if (SensorAlwaysAvailable)
                return true;

            if (!sensor.IsOpen)
            {
                sensor.Open();
            }

            float fWaitTime = Time.realtimeSinceStartup + 3f;
            while (!sensor.IsAvailable && Time.realtimeSinceStartup < fWaitTime)
            {
                // wait for availability
            }

            bool bAvailable = sensor.IsAvailable;

            if (sensor.IsOpen)
            {
                sensor.Close();
            }

            fWaitTime = Time.realtimeSinceStartup + 3f;
            while (sensor.IsOpen && Time.realtimeSinceStartup < fWaitTime)
            {
                // wait for sensor to close
            }

            return bAvailable;
        }

        public int GetSensorsCount()
        {
            int numSensors = 0;

            KinectSensor sensor = KinectSensor.GetDefault();
            if (sensor == null)
                return numSensors;
        
            if (!sensor.IsOpen)
            {
                sensor.Open();
            }

            float fWaitTime = Time.realtimeSinceStartup + 3f;
            while (!sensor.IsAvailable && Time.realtimeSinceStartup < fWaitTime)
            {
                // wait for availability
            }

            numSensors = sensor.IsAvailable ? 1 : 0;

            if (sensor.IsOpen)
            {
                sensor.Close();
            }

            fWaitTime = Time.realtimeSinceStartup + 3f;
            while (sensor.IsOpen && Time.realtimeSinceStartup < fWaitTime)
            {
                // wait for sensor to close
            }

            return numSensors;
        }

        public KinectInterop.SensorData OpenDefaultSensor(KinectInterop.FrameSource dwFlags, float sensorAngle,
            bool bUseMultiSource)
        {
            KinectInterop.SensorData sensorData = new KinectInterop.SensorData();
            _sensorFlags = dwFlags;

            KinectSensor = KinectSensor.GetDefault();
            if (KinectSensor == null)
                return null;

            _coordinateMapper = KinectSensor.CoordinateMapper;

            sensorData.bodyCount =_bodyCount = KinectSensor.BodyFrameSource.BodyCount;
            sensorData.jointCount = 25;

            sensorData.depthCameraFOV = 60f;
            sensorData.colorCameraFOV = 53.8f;
            sensorData.depthCameraOffset = -0.05f;
            sensorData.faceOverlayOffset = -0.04f;

            if ((dwFlags & KinectInterop.FrameSource.TypeBody) != 0)
            {
                if (!bUseMultiSource)
                    _bodyFrameReader = KinectSensor.BodyFrameSource.OpenReader();

                _bodyData = new Body[sensorData.bodyCount];
            }

            var frameDesc = KinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            sensorData.colorImageWidth = frameDesc.Width;
            sensorData.colorImageHeight = frameDesc.Height;

            // flip color image vertically
            sensorData.colorImageScale = new Vector3(1f, -1f, 1f);
            sensorData.depthImageScale = new Vector3(1f, -1f, 1f);

            if ((dwFlags & KinectInterop.FrameSource.TypeColor) != 0)
            {
                if (!bUseMultiSource)
                    _colorFrameReader = KinectSensor.ColorFrameSource.OpenReader();

                sensorData.colorImage = new byte[frameDesc.BytesPerPixel * frameDesc.LengthInPixels];
                _persistentColorBufferPtr = Marshal.AllocHGlobal(sensorData.colorImage.Length);
            }

            sensorData.depthImageWidth = KinectSensor.DepthFrameSource.FrameDescription.Width;
            sensorData.depthImageHeight = KinectSensor.DepthFrameSource.FrameDescription.Height;

            if ((dwFlags & KinectInterop.FrameSource.TypeDepth) != 0)
            {
                if (!bUseMultiSource)
                    _depthFrameReader = KinectSensor.DepthFrameSource.OpenReader();

                sensorData.depthImage = new ushort[KinectSensor.DepthFrameSource.FrameDescription.LengthInPixels];
                _marshalTempDepth = new byte[sensorData.depthImage.Length * sizeof(ushort)];
                _persistentDepthBufferPtr = Marshal.AllocHGlobal(_marshalTempDepth.Length);
            }

            if ((dwFlags & KinectInterop.FrameSource.TypeBodyIndex) != 0)
            {
                if (!bUseMultiSource)
                    _bodyIndexFrameReader = KinectSensor.BodyIndexFrameSource.OpenReader();

                sensorData.bodyIndexImage = new byte[KinectSensor.BodyIndexFrameSource.FrameDescription.LengthInPixels];
                _persistentBodyIndexBufferPtr = Marshal.AllocHGlobal(sensorData.bodyIndexImage.Length);
            }

            if ((dwFlags & KinectInterop.FrameSource.TypeInfrared) != 0)
            {
                if (!bUseMultiSource)
                    _infraredFrameReader = KinectSensor.InfraredFrameSource.OpenReader();

                sensorData.infraredImage = new ushort[KinectSensor.InfraredFrameSource.FrameDescription.LengthInPixels];
                _marshalTempInfrared = new byte[sensorData.infraredImage.Length * sizeof(ushort)];
                _persistentInfraredBufferPtr = Marshal.AllocHGlobal(_marshalTempInfrared.Length);
            }

            //if(!kinectSensor.IsOpen)
            {
                //Debug.Log("Opening sensor, available: " + kinectSensor.IsAvailable);
                KinectSensor.Open();
            }

            float fWaitTime = Time.realtimeSinceStartup + 3f;
            while (!KinectSensor.IsAvailable && Time.realtimeSinceStartup < fWaitTime)
            {
                // wait for sensor to be available
            }

            //fWaitTime = Time.realtimeSinceStartup + 3f;
            while (!KinectSensor.IsOpen && Time.realtimeSinceStartup < fWaitTime)
            {
                // wait for sensor to open
            }

            Debug.Log("K2-sensor " + (KinectSensor.IsOpen ? "opened" : "closed") +
                      ", available: " + KinectSensor.IsAvailable);

            if (bUseMultiSource && dwFlags != KinectInterop.FrameSource.TypeNone && KinectSensor.IsOpen)
            {
                _multiSourceFrameReader = KinectSensor.OpenMultiSourceFrameReader((FrameSourceTypes)((int)dwFlags & 0x3F));
            }

            return sensorData;
        }

        public void CloseSensor(KinectInterop.SensorData sensorData)
        {
            DisposePersistentBuffer(ref _persistentColorBufferPtr);
            DisposePersistentBuffer(ref _persistentDepthBufferPtr);
            DisposePersistentBuffer(ref _persistentBodyIndexBufferPtr);
            DisposePersistentBuffer(ref _persistentInfraredBufferPtr);
            _coordinateMapper = null;

            _bodyFrameReader?.Dispose();
            _bodyFrameReader = null;
        
            _bodyIndexFrameReader?.Dispose();
            _bodyIndexFrameReader = null;


            _colorFrameReader?.Dispose();
            _colorFrameReader = null;
        
            _depthFrameReader?.Dispose();
            _depthFrameReader = null;

            _infraredFrameReader?.Dispose();
            _infraredFrameReader = null;
        
            _multiSourceFrameReader?.Dispose();
            _multiSourceFrameReader = null;

            if (KinectSensor == null)
                return;

            KinectSensor.Close();

            float fWaitTime = Time.realtimeSinceStartup + 3f;
            while (KinectSensor.IsOpen && Time.realtimeSinceStartup < fWaitTime)
            {
                // wait for sensor to close
            }

            Debug.Log("K2-sensor " + (KinectSensor.IsOpen ? "opened" : "closed") +
                      ", available: " + KinectSensor.IsAvailable);

            KinectSensor = null;
        }

        private static void DisposePersistentBuffer(ref IntPtr buffer)
        {
            if (buffer == IntPtr.Zero)
                return;

            Marshal.FreeHGlobal(buffer);
            buffer = IntPtr.Zero;
        }

        public bool UpdateSensorData(KinectInterop.SensorData sensorData) => true;

        public bool GetMultiSourceFrame(KinectInterop.SensorData sensorData)
        {
            if (_multiSourceFrameReader == null)
                return false;
        
            _multiSourceFrame = _multiSourceFrameReader.AcquireLatestFrame();

            if (_multiSourceFrame == null)
                return false;
        
            // try to get all frames at once
            _msBodyFrame = (_sensorFlags & KinectInterop.FrameSource.TypeBody) != 0
                ? _multiSourceFrame.BodyFrameReference.AcquireFrame()
                : null;
        
            _msBodyIndexFrame = (_sensorFlags & KinectInterop.FrameSource.TypeBodyIndex) != 0
                ? _multiSourceFrame.BodyIndexFrameReference.AcquireFrame()
                : null;
        
            _msColorFrame = (_sensorFlags & KinectInterop.FrameSource.TypeColor) != 0
                ? _multiSourceFrame.ColorFrameReference.AcquireFrame()
                : null;
        
            _msDepthFrame = (_sensorFlags & KinectInterop.FrameSource.TypeDepth) != 0
                ? _multiSourceFrame.DepthFrameReference.AcquireFrame()
                : null;
        
            _msInfraredFrame = (_sensorFlags & KinectInterop.FrameSource.TypeInfrared) != 0
                ? _multiSourceFrame.InfraredFrameReference.AcquireFrame()
                : null;

            bool bAllSet =
                ((_sensorFlags & KinectInterop.FrameSource.TypeBody) == 0 || _msBodyFrame != null) &&
                ((_sensorFlags & KinectInterop.FrameSource.TypeBodyIndex) == 0 || _msBodyIndexFrame != null) &&
                ((_sensorFlags & KinectInterop.FrameSource.TypeColor) == 0 || _msColorFrame != null) &&
                ((_sensorFlags & KinectInterop.FrameSource.TypeDepth) == 0 || _msDepthFrame != null) &&
                ((_sensorFlags & KinectInterop.FrameSource.TypeInfrared) == 0 || _msInfraredFrame != null);

            if (bAllSet)
                return _multiSourceFrame != null;
        
            // release all frames
            if (_msBodyFrame != null)
            {
                _msBodyFrame.Dispose();
                _msBodyFrame = null;
            }

            if (_msBodyIndexFrame != null)
            {
                _msBodyIndexFrame.Dispose();
                _msBodyIndexFrame = null;
            }

            if (_msColorFrame != null)
            {
                _msColorFrame.Dispose();
                _msColorFrame = null;
            }

            if (_msDepthFrame != null)
            {
                _msDepthFrame.Dispose();
                _msDepthFrame = null;
            }

            if (_msInfraredFrame != null)
            {
                _msInfraredFrame.Dispose();
                _msInfraredFrame = null;
            }

            return _multiSourceFrame != null;

        }

        public void FreeMultiSourceFrame(KinectInterop.SensorData sensorData)
        {
            // release all frames
            _msBodyFrame?.Dispose();
            _msBodyFrame = null;

            _msBodyIndexFrame?.Dispose();
            _msBodyIndexFrame = null;

            _msColorFrame?.Dispose();
            _msColorFrame = null;

            _msDepthFrame?.Dispose();
            _msDepthFrame = null;

            _msInfraredFrame?.Dispose();
            _msInfraredFrame = null;
        
            _multiSourceFrame = null;
        }

        public bool PollBodyFrame(KinectInterop.SensorData sensorData, ref KinectInterop.BodyFrameData bodyFrame,
            ref Matrix4x4 kinectToWorld, bool bIgnoreJointZ)
        {

            if ((_multiSourceFrameReader == null || _multiSourceFrame == null) &&
                _bodyFrameReader == null)
                return false;

            BodyFrame frame = _multiSourceFrame != null ? _msBodyFrame : _bodyFrameReader.AcquireLatestFrame();

            if (frame == null)
                return false;

            try
            {
                frame.GetAndRefreshBodyData(_bodyData);

                bodyFrame.liPreviousTime = bodyFrame.liRelativeTime;
                bodyFrame.liRelativeTime = frame.RelativeTime.Ticks;

                if (sensorData.hintHeightAngle)
                {
                    // get the floor plane
                    Windows.Kinect.Vector4 vFloorPlane = frame.FloorClipPlane;
                    Vector3 floorPlane = new Vector3(vFloorPlane.X, vFloorPlane.Y, vFloorPlane.Z);

                    sensorData.sensorRotDetected = Quaternion.FromToRotation(floorPlane, Vector3.up);
                    sensorData.sensorHgtDetected = vFloorPlane.W;
                }
            }
            finally
            {
                frame.Dispose();
            }

            for (int i = 0; i < sensorData.bodyCount; i++)
            {
                Body body = _bodyData[i];

                if (body == null)
                {
                    bodyFrame.bodyData[i].bIsTracked = 0;
                    continue;
                }

                bodyFrame.bodyData[i].bIsTracked = (short)(body.IsTracked ? 1 : 0);

                if (!body.IsTracked)
                    continue;
            
                // transfer body and joints data
                bodyFrame.bodyData[i].liTrackingID = (long)body.TrackingId;

                // cache the body joints (following the advice of Brian Chasalow)
                var bodyJoints = body.Joints;

                for (int j = 0; j < sensorData.jointCount; j++)
                {
                    Windows.Kinect.Joint joint = bodyJoints[(Windows.Kinect.JointType)j];
                    KinectInterop.JointData jointData = bodyFrame.bodyData[i].joint[j];

                    //jointData.jointType = (KinectInterop.JointType)j;
                    jointData.trackingState = (KinectInterop.TrackingState)joint.TrackingState;

                    if ((int)joint.TrackingState != (int)TrackingState.NotTracked)
                    {
                        float jPosZ = (bIgnoreJointZ && j > 0)
                            ? bodyFrame.bodyData[i].joint[0].kinectPos.z
                            : joint.Position.Z;
                        jointData.kinectPos = new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);
                        jointData.position =
                            kinectToWorld.MultiplyPoint3x4(new Vector3(joint.Position.X, joint.Position.Y,
                                jPosZ));
                    }

                    jointData.orientation = Quaternion.identity;
//							Windows.Kinect.Vector4 vQ = body.JointOrientations[jointData.jointType].Orientation;
//							jointData.orientation = new Quaternion(vQ.X, vQ.Y, vQ.Z, vQ.W);

                    if (j == 0)
                    {
                        bodyFrame.bodyData[i].kinectPos = jointData.kinectPos;
                        bodyFrame.bodyData[i].position = jointData.position;
                        bodyFrame.bodyData[i].orientation = jointData.orientation;
                    }

                    bodyFrame.bodyData[i].joint[j] = jointData;
                }

                // tranfer hand states
                bodyFrame.bodyData[i].leftHandState = (KinectInterop.HandState)body.HandLeftState;
                bodyFrame.bodyData[i].leftHandConfidence =
                    (KinectInterop.TrackingConfidence)body.HandLeftConfidence;

                bodyFrame.bodyData[i].rightHandState = (KinectInterop.HandState)body.HandRightState;
                bodyFrame.bodyData[i].rightHandConfidence =
                    (KinectInterop.TrackingConfidence)body.HandRightConfidence;
            }

            return true;
        }

        public bool PollColorFrame(KinectInterop.SensorData sensorData)
        {
            if ((_multiSourceFrameReader == null || _multiSourceFrame == null) && _colorFrameReader == null)
                return false;


            ColorFrame colorFrame = _multiSourceFrame != null ? _msColorFrame : _colorFrameReader.AcquireLatestFrame();

            if (colorFrame == null)
                return false;

            try
            {
                // копируем данные кадра в буфер
                colorFrame.CopyConvertedFrameDataToIntPtr(_persistentColorBufferPtr, (uint)sensorData.colorImage.Length,
                    ColorImageFormat.Rgba);

                // копируем данные из временного буфера в массив
                Marshal.Copy(_persistentColorBufferPtr, sensorData.colorImage, 0, sensorData.colorImage.Length);
            }
            catch (Exception e)
            {
                Debug.LogError($"[PollColorFrame]: Marshal Copy Error: {e.Message}");
                return false;
            }
            finally
            {
                sensorData.lastColorFrameTime = colorFrame.RelativeTime.Ticks;
                colorFrame.Dispose();
            }

            return true;
        }

        public bool PollDepthFrame(KinectInterop.SensorData sensorData)
        {
            bool bNewFrame = PollDepthOnly(sensorData);
            bNewFrame = bNewFrame && PollBodyIndexOnly(sensorData);
            return bNewFrame;
        }

        private bool PollDepthOnly(KinectInterop.SensorData sensorData)
        {
            if ((_multiSourceFrameReader == null || _multiSourceFrame == null) && _depthFrameReader == null)
                return false;

            DepthFrame depthFrame = _multiSourceFrame != null ? _msDepthFrame : _depthFrameReader.AcquireLatestFrame();

            if (depthFrame == null)
                return false;

            try
            {
                // копируем данные кадра в буфер
                depthFrame.CopyFrameDataToIntPtr(_persistentDepthBufferPtr, (uint)_marshalTempDepth.Length);

                // копируем данные из временного буфера в массив
                Marshal.Copy(_persistentDepthBufferPtr, _marshalTempDepth, 0, _marshalTempDepth.Length);

                Buffer.BlockCopy(_marshalTempDepth, 0, sensorData.depthImage, 0, _marshalTempDepth.Length);
            }
            catch (Exception e)
            {
                Debug.LogError($"[PollDepthFrame/Depth]: Marshal Copy Error: {e.Message}");
                return false;
            }
            finally
            {
                sensorData.lastDepthFrameTime = depthFrame.RelativeTime.Ticks;
                depthFrame.Dispose();
            }

            return true;
        }

        private bool PollBodyIndexOnly(KinectInterop.SensorData sensorData)
        {
            if ((_multiSourceFrameReader == null || _multiSourceFrame == null) && _bodyIndexFrameReader == null)
                return false;

            BodyIndexFrame bodyIndexFrame = _multiSourceFrame != null
                ? _msBodyIndexFrame
                : _bodyIndexFrameReader.AcquireLatestFrame();

            if (bodyIndexFrame == null)
                return false;
            try
            {
                // копируем данные кадра в буфер
                bodyIndexFrame.CopyFrameDataToIntPtr(_persistentBodyIndexBufferPtr, (uint)sensorData.bodyIndexImage.Length);

                // копируем данные из временного буфера в массив
                Marshal.Copy(_persistentBodyIndexBufferPtr, sensorData.bodyIndexImage, 0, sensorData.bodyIndexImage.Length);
            }
            catch (Exception e)
            {
                Debug.LogError($"[PollDepthFrame/BodyIndex]: Marshal Copy Error: {e.Message}");
                return false;
            }
            finally
            {
                sensorData.lastBodyIndexFrameTime = bodyIndexFrame.RelativeTime.Ticks;
                bodyIndexFrame.Dispose();
            }

            return true;
        }

        public bool PollInfraredFrame(KinectInterop.SensorData sensorData)
        {
            if ((_multiSourceFrameReader == null || _multiSourceFrame == null) && _infraredFrameReader == null)
                return false;

            InfraredFrame infraredFrame =
                _multiSourceFrame != null ? _msInfraredFrame : _infraredFrameReader.AcquireLatestFrame();

            if (infraredFrame == null)
                return false;

            try
            {
                // копируем данные кадра в буфер
                infraredFrame.CopyFrameDataToIntPtr(_persistentInfraredBufferPtr, (uint)_marshalTempInfrared.Length);

                // копируем данные из временного буфера в массив
                Marshal.Copy(_persistentInfraredBufferPtr, _marshalTempInfrared, 0, _marshalTempInfrared.Length);

                Buffer.BlockCopy(_marshalTempInfrared, 0, sensorData.infraredImage, 0, _marshalTempInfrared.Length);
            }
            catch (Exception e)
            {
                Debug.LogError($"[PollInfraredFrame]: Marshal Copy Error: {e.Message}");
                return false;
            }
            finally
            {
                sensorData.lastInfraredFrameTime = infraredFrame.RelativeTime.Ticks;
                infraredFrame.Dispose();
            }

            return true;
        }

        public void FixJointOrientations(KinectInterop.SensorData sensorData, ref KinectInterop.BodyData bodyData)
        {
            // no fixes are needed
        }

        public bool IsBodyTurned(ref KinectInterop.BodyData bodyData)
        {
            //face = On: Face (357.0/1.0)
            //face = Off
            //|   Head_px <= -0.02
            //|   |   Neck_dx <= 0.08: Face (46.0/1.0)
            //|   |   Neck_dx > 0.08: Back (3.0)
            //|   Head_px > -0.02
            //|   |   SpineShoulder_px <= -0.02: Face (4.0)
            //|   |   SpineShoulder_px > -0.02: Back (64.0/1.0)

            bool bBodyTurned = false;

            if (!_bFaceTrackingInit)
                return false;
        
            bool bFaceOn = IsFaceTracked(bodyData.liTrackingID);
        
            if (bFaceOn)
                return false;
        
            // face = Off
            if (bodyData.joint[(int)KinectInterop.JointType.Head].posRel.x <= -0.02f)
            {
                bBodyTurned = (bodyData.joint[(int)KinectInterop.JointType.Neck].posVel.x > 0.08f);
            }
            else
            {
                // Head_px > -0.02
                bBodyTurned = (bodyData.joint[(int)KinectInterop.JointType.SpineShoulder].posRel.x > -0.02f);
            }

            return bBodyTurned;
        }

        public Vector2 MapSpacePointToDepthCoords(KinectInterop.SensorData sensorData, Vector3 spacePos)
        {
            Vector2 vPoint = Vector2.zero;

            if (_coordinateMapper != null)
            {
                CameraSpacePoint camPoint = new CameraSpacePoint();
                camPoint.X = spacePos.x;
                camPoint.Y = spacePos.y;
                camPoint.Z = spacePos.z;

                CameraSpacePoint[] camPoints = new CameraSpacePoint[1];
                camPoints[0] = camPoint;

                DepthSpacePoint[] depthPoints = new DepthSpacePoint[1];
                _coordinateMapper.MapCameraPointsToDepthSpace(camPoints, depthPoints);

                DepthSpacePoint depthPoint = depthPoints[0];

                if (depthPoint.X >= 0 && depthPoint.X < sensorData.depthImageWidth &&
                    depthPoint.Y >= 0 && depthPoint.Y < sensorData.depthImageHeight)
                {
                    vPoint.x = depthPoint.X;
                    vPoint.y = depthPoint.Y;
                }
            }

            return vPoint;
        }

        public Vector3 MapDepthPointToSpaceCoords(KinectInterop.SensorData sensorData, Vector2 depthPos, ushort depthVal)
        {
            Vector3 vPoint = Vector3.zero;

            if (_coordinateMapper != null && depthPos != Vector2.zero)
            {
                DepthSpacePoint depthPoint = new DepthSpacePoint();
                depthPoint.X = depthPos.x;
                depthPoint.Y = depthPos.y;

                DepthSpacePoint[] depthPoints = new DepthSpacePoint[1];
                depthPoints[0] = depthPoint;

                ushort[] depthVals = new ushort[1];
                depthVals[0] = depthVal;

                CameraSpacePoint[] camPoints = new CameraSpacePoint[1];
                _coordinateMapper.MapDepthPointsToCameraSpace(depthPoints, depthVals, camPoints);

                CameraSpacePoint camPoint = camPoints[0];
                vPoint.x = camPoint.X;
                vPoint.y = camPoint.Y;
                vPoint.z = camPoint.Z;
            }

            return vPoint;
        }

        public bool MapDepthFrameToSpaceCoords(KinectInterop.SensorData sensorData, ref Vector3[] vSpaceCoords)
        {
            if (_coordinateMapper != null && sensorData.depthImage != null)
            {
                var pDepthData = GCHandle.Alloc(sensorData.depthImage, GCHandleType.Pinned);
                var pSpaceCoordsData = GCHandle.Alloc(vSpaceCoords, GCHandleType.Pinned);

                _coordinateMapper.MapDepthFrameToCameraSpaceUsingIntPtr(
                    pDepthData.AddrOfPinnedObject(),
                    sensorData.depthImage.Length * sizeof(ushort),
                    pSpaceCoordsData.AddrOfPinnedObject(),
                    (uint)vSpaceCoords.Length);

                pSpaceCoordsData.Free();
                pDepthData.Free();

                return true;
            }

            return false;
        }

        public Vector2 MapDepthPointToColorCoords(KinectInterop.SensorData sensorData, Vector2 depthPos, ushort depthVal)
        {
            Vector2 vPoint = Vector2.zero;

            if (_coordinateMapper != null && depthPos != Vector2.zero)
            {
                DepthSpacePoint depthPoint = new DepthSpacePoint();
                depthPoint.X = depthPos.x;
                depthPoint.Y = depthPos.y;

                DepthSpacePoint[] depthPoints = new DepthSpacePoint[1];
                depthPoints[0] = depthPoint;

                ushort[] depthVals = new ushort[1];
                depthVals[0] = depthVal;

                ColorSpacePoint[] colPoints = new ColorSpacePoint[1];
                _coordinateMapper.MapDepthPointsToColorSpace(depthPoints, depthVals, colPoints);

                ColorSpacePoint colPoint = colPoints[0];
                vPoint.x = colPoint.X;
                vPoint.y = colPoint.Y;
            }

            return vPoint;
        }

        public bool MapDepthFrameToColorCoords(KinectInterop.SensorData sensorData, ref Vector2[] vColorCoords)
        {
            if (_coordinateMapper != null && sensorData.colorImage != null && sensorData.depthImage != null)
            {
                var pDepthData = GCHandle.Alloc(sensorData.depthImage, GCHandleType.Pinned);
                var pColorCoordsData = GCHandle.Alloc(vColorCoords, GCHandleType.Pinned);

                _coordinateMapper.MapDepthFrameToColorSpaceUsingIntPtr(
                    pDepthData.AddrOfPinnedObject(),
                    sensorData.depthImage.Length * sizeof(ushort),
                    pColorCoordsData.AddrOfPinnedObject(),
                    (uint)vColorCoords.Length);

                pColorCoordsData.Free();
                pDepthData.Free();

                return true;
            }

            return false;
        }

        public bool MapColorFrameToDepthCoords(KinectInterop.SensorData sensorData, ref Vector2[] vDepthCoords)
        {
            if (_coordinateMapper != null && sensorData.colorImage != null && sensorData.depthImage != null)
            {
                var pDepthData = GCHandle.Alloc(sensorData.depthImage, GCHandleType.Pinned);
                var pDepthCoordsData = GCHandle.Alloc(vDepthCoords, GCHandleType.Pinned);

                _coordinateMapper.MapColorFrameToDepthSpaceUsingIntPtr(
                    pDepthData.AddrOfPinnedObject(),
                    (uint)sensorData.depthImage.Length * sizeof(ushort),
                    pDepthCoordsData.AddrOfPinnedObject(),
                    (uint)vDepthCoords.Length);

                pDepthCoordsData.Free();
                pDepthData.Free();

                return true;
            }

            return false;
        }

//	// returns the index of the given joint in joint's array or -1 if joint is not applicable
//	public int GetJointIndex(KinectInterop.JointType joint)
//	{
//		return (int)joint;
//	}

//	// returns the joint at given index
//	public KinectInterop.JointType GetJointAtIndex(int index)
//	{
//		return (KinectInterop.JointType)(index);
//	}

//	// returns the parent joint of the given joint
//	public KinectInterop.JointType GetParentJoint(KinectInterop.JointType joint)
//	{
//		switch(joint)
//		{
//			case KinectInterop.JointType.SpineBase:
//				return KinectInterop.JointType.SpineBase;
//				
//			case KinectInterop.JointType.Neck:
//				return KinectInterop.JointType.SpineShoulder;
//				
//			case KinectInterop.JointType.SpineShoulder:
//				return KinectInterop.JointType.SpineMid;
//				
//			case KinectInterop.JointType.ShoulderLeft:
//			case KinectInterop.JointType.ShoulderRight:
//				return KinectInterop.JointType.SpineShoulder;
//				
//			case KinectInterop.JointType.HipLeft:
//			case KinectInterop.JointType.HipRight:
//				return KinectInterop.JointType.SpineBase;
//				
//			case KinectInterop.JointType.HandTipLeft:
//				return KinectInterop.JointType.HandLeft;
//				
//			case KinectInterop.JointType.ThumbLeft:
//				return KinectInterop.JointType.WristLeft;
//			
//			case KinectInterop.JointType.HandTipRight:
//				return KinectInterop.JointType.HandRight;
//
//			case KinectInterop.JointType.ThumbRight:
//				return KinectInterop.JointType.WristRight;
//		}
//			
//		return (KinectInterop.JointType)((int)joint - 1);
//	}

//	// returns the next joint in the hierarchy, as to the given joint
//	public KinectInterop.JointType GetNextJoint(KinectInterop.JointType joint)
//	{
//		switch(joint)
//		{
//			case KinectInterop.JointType.SpineBase:
//				return KinectInterop.JointType.SpineMid;
//			case KinectInterop.JointType.SpineMid:
//				return KinectInterop.JointType.SpineShoulder;
//			case KinectInterop.JointType.SpineShoulder:
//				return KinectInterop.JointType.Neck;
//			case KinectInterop.JointType.Neck:
//				return KinectInterop.JointType.Head;
//				
//			case KinectInterop.JointType.ShoulderLeft:
//				return KinectInterop.JointType.ElbowLeft;
//			case KinectInterop.JointType.ElbowLeft:
//				return KinectInterop.JointType.WristLeft;
//			case KinectInterop.JointType.WristLeft:
//				return KinectInterop.JointType.HandLeft;
//			case KinectInterop.JointType.HandLeft:
//				return KinectInterop.JointType.HandTipLeft;
//				
//			case KinectInterop.JointType.ShoulderRight:
//				return KinectInterop.JointType.ElbowRight;
//			case KinectInterop.JointType.ElbowRight:
//				return KinectInterop.JointType.WristRight;
//			case KinectInterop.JointType.WristRight:
//				return KinectInterop.JointType.HandRight;
//			case KinectInterop.JointType.HandRight:
//				return KinectInterop.JointType.HandTipRight;
//				
//			case KinectInterop.JointType.HipLeft:
//				return KinectInterop.JointType.KneeLeft;
//			case KinectInterop.JointType.KneeLeft:
//				return KinectInterop.JointType.AnkleLeft;
//			case KinectInterop.JointType.AnkleLeft:
//				return KinectInterop.JointType.FootLeft;
//				
//			case KinectInterop.JointType.HipRight:
//				return KinectInterop.JointType.KneeRight;
//			case KinectInterop.JointType.KneeRight:
//				return KinectInterop.JointType.AnkleRight;
//			case KinectInterop.JointType.AnkleRight:
//				return KinectInterop.JointType.FootRight;
//		}
//		
//		return joint;  // in case of end joint - Head, HandTipLeft, HandTipRight, FootLeft, FootRight
//	}

        public bool IsFaceTrackingAvailable(ref bool bNeedRestart)
        {
            bool bOneCopied = false, bAllCopied = true;
            string sTargetPath;

            if (!KinectInterop.Is64bitArchitecture())
            {
                // 32 bit
                sTargetPath = KinectInterop.GetTargetDllPath(".", false) + "/";

                Dictionary<string, string> dictFilesToUnzip = new Dictionary<string, string>();
                dictFilesToUnzip["Kinect20.Face.dll"] = sTargetPath + "Kinect20.Face.dll";
                dictFilesToUnzip["KinectFaceUnityAddin.dll"] = sTargetPath + "KinectFaceUnityAddin.dll";
                dictFilesToUnzip["msvcp110.dll"] = sTargetPath + "msvcp110.dll";
                dictFilesToUnzip["msvcr110.dll"] = sTargetPath + "msvcr110.dll";

                KinectInterop.UnzipResourceFiles(dictFilesToUnzip, "KinectV2UnityAddin.x86.zip", ref bOneCopied,
                    ref bAllCopied);
            }
            else
            {
                //Debug.Log("Face - x64-architecture.");
                sTargetPath = KinectInterop.GetTargetDllPath(".", true) + "/";

                Dictionary<string, string> dictFilesToUnzip = new Dictionary<string, string>();
                dictFilesToUnzip["Kinect20.Face.dll"] = sTargetPath + "Kinect20.Face.dll";
                dictFilesToUnzip["KinectFaceUnityAddin.dll"] = sTargetPath + "KinectFaceUnityAddin.dll";
                dictFilesToUnzip["msvcp110.dll"] = sTargetPath + "msvcp110.dll";
                dictFilesToUnzip["msvcr110.dll"] = sTargetPath + "msvcr110.dll";

                KinectInterop.UnzipResourceFiles(dictFilesToUnzip, "KinectV2UnityAddin.x64.zip", ref bOneCopied,
                    ref bAllCopied);
            }

            KinectInterop.UnzipResourceDirectory(sTargetPath, "NuiDatabase.zip", sTargetPath + "NuiDatabase");

            bNeedRestart = (bOneCopied && bAllCopied);

            return true;
        }

        public bool InitFaceTracking(bool bUseFaceModel, bool bDrawFaceRect)
        {
            _isDrawFaceRect = bDrawFaceRect;

//		// load the native dlls to make sure libraries are loaded (after previous finish-unload)
//		KinectInterop.LoadNativeLib("Kinect20.Face.dll");
//		KinectInterop.LoadNativeLib("KinectFaceUnityAddin.dll");

            // specify the required face frame results
            FaceFrameFeatures faceFrameFeatures =
                    FaceFrameFeatures.BoundingBoxInColorSpace
                    //| FaceFrameFeatures.BoundingBoxInInfraredSpace
                    | FaceFrameFeatures.PointsInColorSpace
                    //| FaceFrameFeatures.PointsInInfraredSpace
                    | FaceFrameFeatures.RotationOrientation
                    | FaceFrameFeatures.FaceEngagement
                    | FaceFrameFeatures.Glasses
                    | FaceFrameFeatures.Happy
                    | FaceFrameFeatures.LeftEyeClosed
                    | FaceFrameFeatures.RightEyeClosed
                    | FaceFrameFeatures.LookingAway
                    | FaceFrameFeatures.MouthMoved
                    | FaceFrameFeatures.MouthOpen
                ;

            // create a face frame source + reader to track each face in the FOV
            FaceFrameSources = new FaceFrameSource[this._bodyCount];
            FaceFrameReaders = new FaceFrameReader[this._bodyCount];

            if (bUseFaceModel)
            {
                HdFaceFrameSources = new HighDefinitionFaceFrameSource[this._bodyCount];
                HdFaceFrameReaders = new HighDefinitionFaceFrameReader[this._bodyCount];

                HdFaceModels = new FaceModel[this._bodyCount];
                HdFaceAlignments = new FaceAlignment[this._bodyCount];
            }

            for (int i = 0; i < _bodyCount; i++)
            {
                // create the face frame source with the required face frame features and an initial tracking Id of 0
                FaceFrameSources[i] = FaceFrameSource.Create(this.KinectSensor, 0, faceFrameFeatures);

                // open the corresponding reader
                FaceFrameReaders[i] = FaceFrameSources[i].OpenReader();

                if (bUseFaceModel)
                {
                    ///////// HD Face
                    HdFaceFrameSources[i] = HighDefinitionFaceFrameSource.Create(this.KinectSensor);
                    HdFaceFrameReaders[i] = HdFaceFrameSources[i].OpenReader();

                    HdFaceModels[i] = FaceModel.Create();
                    HdFaceAlignments[i] = FaceAlignment.Create();
                }
            }

            // allocate storage to store face frame results for each face in the FOV
            FaceFrameResults = new FaceFrameResult[this._bodyCount];

//		FrameDescription frameDescription = this.kinectSensor.ColorFrameSource.FrameDescription;
//		faceDisplayWidth = frameDescription.Width;
//		faceDisplayHeight = frameDescription.Height;

            _bFaceTrackingInit = true;

            return _bFaceTrackingInit;
        }

        public void FinishFaceTracking()
        {
            if (FaceFrameReaders != null)
            {
                for (int i = 0; i < FaceFrameReaders.Length; i++)
                {
                    if (FaceFrameReaders[i] != null)
                    {
                        FaceFrameReaders[i].Dispose();
                        FaceFrameReaders[i] = null;
                    }
                }
            }

            if (FaceFrameSources != null)
            {
                for (int i = 0; i < FaceFrameSources.Length; i++)
                {
                    FaceFrameSources[i] = null;
                }
            }

            ///////// HD Face
            if (HdFaceFrameSources != null)
            {
                for (int i = 0; i < HdFaceAlignments.Length; i++)
                {
                    HdFaceAlignments[i] = null;
                }

                for (int i = 0; i < HdFaceModels.Length; i++)
                {
                    if (HdFaceModels[i] != null)
                    {
                        HdFaceModels[i].Dispose();
                        HdFaceModels[i] = null;
                    }
                }

                for (int i = 0; i < HdFaceFrameReaders.Length; i++)
                {
                    if (HdFaceFrameReaders[i] != null)
                    {
                        HdFaceFrameReaders[i].Dispose();
                        HdFaceFrameReaders[i] = null;
                    }
                }

                for (int i = 0; i < HdFaceFrameSources.Length; i++)
                {
                    //hdFaceFrameSources[i].Dispose(true);
                    HdFaceFrameSources[i] = null;
                }
            }

            _bFaceTrackingInit = false;

//		// unload the native dlls to prevent hd-face-wrapper's memory leaks
//		KinectInterop.DeleteNativeLib("KinectFaceUnityAddin.dll", true);
//		KinectInterop.DeleteNativeLib("Kinect20.Face.dll", true);
        }

        public bool UpdateFaceTracking()
        {
            if (_bodyData == null || FaceFrameSources == null || FaceFrameReaders == null)
                return false;

            for (int i = 0; i < this._bodyCount; i++)
            {
                if (FaceFrameSources[i] != null)
                {
                    if (!FaceFrameSources[i].IsTrackingIdValid)
                    {
                        FaceFrameSources[i].TrackingId = 0;
                    }

                    if (_bodyData[i] != null && _bodyData[i].IsTracked)
                    {
                        FaceFrameSources[i].TrackingId = _bodyData[i].TrackingId;
                    }
                }

                if (FaceFrameReaders[i] != null)
                {
                    FaceFrame faceFrame = FaceFrameReaders[i].AcquireLatestFrame();

                    if (faceFrame != null)
                    {
                        int index = GetFaceSourceIndex(faceFrame.FaceFrameSource);

                        if (ValidateFaceBox(faceFrame.FaceFrameResult))
                        {
                            FaceFrameResults[index] = faceFrame.FaceFrameResult;
                        }
                        else
                        {
                            FaceFrameResults[index] = null;
                        }

                        faceFrame.Dispose();
                    }
                }

                ///////// HD Face
                if (HdFaceFrameSources != null && HdFaceFrameSources[i] != null)
                {
                    if (!HdFaceFrameSources[i].IsTrackingIdValid)
                    {
                        HdFaceFrameSources[i].TrackingId = 0;
                    }

                    if (_bodyData[i] != null && _bodyData[i].IsTracked)
                    {
                        HdFaceFrameSources[i].TrackingId = _bodyData[i].TrackingId;
                    }
                }

                if (HdFaceFrameReaders != null && HdFaceFrameReaders[i] != null)
                {
                    HighDefinitionFaceFrame hdFaceFrame = HdFaceFrameReaders[i].AcquireLatestFrame();

                    if (hdFaceFrame != null)
                    {
                        if (hdFaceFrame.IsFaceTracked && (HdFaceAlignments[i] != null))
                        {
                            hdFaceFrame.GetAndRefreshFaceAlignmentResult(HdFaceAlignments[i]);
                        }

                        hdFaceFrame.Dispose();
                    }
                }
            }

            return true;
        }

        private int GetFaceSourceIndex(FaceFrameSource faceFrameSource)
        {
            int index = -1;

            for (int i = 0; i < this._bodyCount; i++)
            {
                if (this.FaceFrameSources[i] == faceFrameSource)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        private bool ValidateFaceBox(FaceFrameResult faceResult)
        {
            bool isFaceValid = faceResult != null;

            if (isFaceValid)
            {
                var faceBox = faceResult.FaceBoundingBoxInColorSpace;
                //if (faceBox != null)
                {
                    // check if we have a valid rectangle within the bounds of the screen space
                    isFaceValid = (faceBox.Right - faceBox.Left) > 0 &&
                                  (faceBox.Bottom - faceBox.Top) > 0; // &&
                    //faceBox.Right <= this.faceDisplayWidth &&
                    //faceBox.Bottom <= this.faceDisplayHeight;
                }
            }

            return isFaceValid;
        }

        public bool IsFaceTrackingActive()
        {
            return _bFaceTrackingInit;
        }

        public bool IsDrawFaceRect()
        {
            return _isDrawFaceRect;
        }

        public bool IsFaceTracked(long userId)
        {
            for (int i = 0; i < this._bodyCount; i++)
            {
                if (FaceFrameSources != null && FaceFrameSources[i] != null &&
                    FaceFrameSources[i].TrackingId == (ulong)userId)
                {
                    if (FaceFrameResults != null && FaceFrameResults[i] != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool GetFaceRect(long userId, ref Rect faceRect)
        {
            for (int i = 0; i < this._bodyCount; i++)
            {
                if (FaceFrameSources != null && FaceFrameSources[i] != null &&
                    FaceFrameSources[i].TrackingId == (ulong)userId)
                {
                    if (FaceFrameResults != null && FaceFrameResults[i] != null)
                    {
                        var faceBox = FaceFrameResults[i].FaceBoundingBoxInColorSpace;

                        //if (faceBox != null)
                        {
                            faceRect.x = faceBox.Left;
                            faceRect.y = faceBox.Top;
                            faceRect.width = faceBox.Right - faceBox.Left;
                            faceRect.height = faceBox.Bottom - faceBox.Top;

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void VisualizeFaceTrackerOnColorTex(Texture2D texColor)
        {
            if (_bFaceTrackingInit)
            {
                for (int i = 0; i < this._bodyCount; i++)
                {
                    if (FaceFrameSources != null && FaceFrameSources[i] != null && FaceFrameSources[i].IsTrackingIdValid)
                    {
                        if (FaceFrameResults != null && FaceFrameResults[i] != null)
                        {
                            var faceBox = FaceFrameResults[i].FaceBoundingBoxInColorSpace;

                            //if (faceBox != null)
                            {
                                UnityEngine.Color color = UnityEngine.Color.magenta;
                                Vector2 pt1, pt2;

                                // bottom
                                pt1.x = faceBox.Left;
                                pt1.y = faceBox.Top;
                                pt2.x = faceBox.Right;
                                pt2.y = pt1.y;
                                DrawLine(texColor, pt1, pt2, color);

                                // right
                                pt1.x = pt2.x;
                                pt1.y = pt2.y;
                                pt2.x = pt1.x;
                                pt2.y = faceBox.Bottom;
                                DrawLine(texColor, pt1, pt2, color);

                                // top
                                pt1.x = pt2.x;
                                pt1.y = pt2.y;
                                pt2.x = faceBox.Left;
                                pt2.y = pt1.y;
                                DrawLine(texColor, pt1, pt2, color);

                                // left
                                pt1.x = pt2.x;
                                pt1.y = pt2.y;
                                pt2.x = pt1.x;
                                pt2.y = faceBox.Top;
                                DrawLine(texColor, pt1, pt2, color);
                            }
                        }
                    }
                }
            }
        }

        private static void DrawLine(Texture2D aTexture, Vector2 ptStart, Vector2 ptEnd, UnityEngine.Color aColor)
        {
            KinectInterop.DrawLine(aTexture, (int)ptStart.x, (int)ptStart.y, (int)ptEnd.x, (int)ptEnd.y, aColor);
        }

        public bool GetHeadPosition(long userId, ref Vector3 headPos)
        {
            if (_bodyData == null || _bodyCount == 0)
                return false;

            for (int i = 0; i < this._bodyCount; i++)
            {
                if (_bodyData[i] != null && _bodyData[i].TrackingId == (ulong)userId && _bodyData[i].IsTracked)
                {
                    CameraSpacePoint vHeadPos = _bodyData[i].Joints[Windows.Kinect.JointType.Head].Position;

                    if (vHeadPos.Z > 0f)
                    {
                        headPos.x = vHeadPos.X;
                        headPos.y = vHeadPos.Y;
                        headPos.z = vHeadPos.Z;

                        return true;
                    }
                }
            }

            return false;
        }

        public bool GetHeadRotation(long userId, ref Quaternion headRot)
        {
            for (int i = 0; i < this._bodyCount; i++)
            {
                if (FaceFrameSources != null && FaceFrameSources[i] != null &&
                    FaceFrameSources[i].TrackingId == (ulong)userId)
                {
                    if (FaceFrameResults != null && FaceFrameResults[i] != null)
                    {
                        Windows.Kinect.Vector4 vHeadRot = FaceFrameResults[i].FaceRotationQuaternion;

                        if (vHeadRot.W > 0f)
                        {
                            headRot = new Quaternion(vHeadRot.X, vHeadRot.Y, vHeadRot.Z, vHeadRot.W);
                            return true;
                        }
//					else
//					{
//						Debug.Log(string.Format("Bad rotation: ({0:F2}, {1:F2}, {2:F2}, {3:F2}})", vHeadRot.X, vHeadRot.Y, vHeadRot.Z, vHeadRot.W));
//						return false;
//					}
                    }
                }
            }

            return false;
        }

        public bool GetAnimUnits(long userId, ref Dictionary<KinectInterop.FaceShapeAnimations, float> dictAu)
        {
            for (int i = 0; i < this._bodyCount; i++)
            {
                if (HdFaceFrameSources != null && HdFaceFrameSources[i] != null &&
                    HdFaceFrameSources[i].TrackingId == (ulong)userId)
                {
                    if (HdFaceAlignments != null && HdFaceAlignments[i] != null)
                    {
                        foreach (Microsoft.Kinect.Face.FaceShapeAnimations akey in HdFaceAlignments[i].AnimationUnits.Keys)
                        {
                            dictAu[(KinectInterop.FaceShapeAnimations)akey] = HdFaceAlignments[i].AnimationUnits[akey];
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public bool GetShapeUnits(long userId, ref Dictionary<KinectInterop.FaceShapeDeformations, float> dictSu)
        {
            for (int i = 0; i < this._bodyCount; i++)
            {
                if (HdFaceFrameSources != null && HdFaceFrameSources[i] != null &&
                    HdFaceFrameSources[i].TrackingId == (ulong)userId)
                {
                    if (HdFaceModels != null && HdFaceModels[i] != null)
                    {
                        foreach (Microsoft.Kinect.Face.FaceShapeDeformations skey in HdFaceModels[i].FaceShapeDeformations
                                     .Keys)
                        {
                            dictSu[(KinectInterop.FaceShapeDeformations)skey] = HdFaceModels[i].FaceShapeDeformations[skey];
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public bool GetFaceProperties(long userId, ref Dictionary<string, string> faceProps)
        {
            for (int i = 0; i < this._bodyCount; i++)
            {
                if (FaceFrameSources != null && FaceFrameSources[i] != null &&
                    FaceFrameSources[i].TrackingId == (ulong)userId)
                {
                    if (FaceFrameResults != null && FaceFrameResults[i] != null)
                    {
                        var faceFrameProps = FaceFrameResults[i].FaceProperties;

                        foreach (FaceProperty faceProp in faceFrameProps.Keys)
                        {
                            float facePropValue = DetectionResult2Percent(faceFrameProps[faceProp]);

                            if (facePropValue >= 0f)
                            {
                                faceProps[faceProp.ToString().ToLower()] = facePropValue.ToString(CultureInfo.InvariantCulture);
                            }
                        }

                        return (faceProps.Count > 0);
                    }
                }
            }

            return false;
        }


        // converts detection result to percentage (0-1), or -1 in case of Unknown
        private float DetectionResult2Percent(DetectionResult detRes)
        {
            switch(detRes)
            {
                case DetectionResult.No:
                    return 0.0f;
                case DetectionResult.Maybe:
                    return 0.5f;
                case DetectionResult.Yes:
                    return 1.0f;
            }

            return -1.0f;
        }

        public int GetFaceModelVerticesCount(long userId)
        {
            for (int i = 0; i < this._bodyCount; i++)
            {
                if (HdFaceFrameSources != null && HdFaceFrameSources[i] != null &&
                    (HdFaceFrameSources[i].TrackingId == (ulong)userId || userId == 0))
                {
                    if (HdFaceModels != null && HdFaceModels[i] != null)
                    {
                        var vertices = HdFaceModels[i].CalculateVerticesForAlignment(HdFaceAlignments[i]);
                        int verticesCount = vertices.Count;

                        return verticesCount;
                    }
                }
            }

            return 0;
        }

        public bool GetFaceModelVertices(long userId, ref Vector3[] avVertices)
        {
            for (int i = 0; i < this._bodyCount; i++)
            {
                if (HdFaceFrameSources != null && HdFaceFrameSources[i] != null &&
                    (HdFaceFrameSources[i].TrackingId == (ulong)userId || userId == 0))
                {
                    if (HdFaceModels != null && HdFaceModels[i] != null)
                    {
                        var vertices = HdFaceModels[i].CalculateVerticesForAlignment(HdFaceAlignments[i]);
                        int verticesCount = vertices.Count;

                        if (avVertices != null && avVertices.Length == verticesCount)
                        {
                            for (int v = 0; v < verticesCount; v++)
                            {
                                avVertices[v].x = vertices[v].X;
                                avVertices[v].y = vertices[v].Y;
                                avVertices[v].z = vertices[v].Z; // -vertices[v].Z;
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public int GetFaceModelTrianglesCount()
        {
            var triangleIndices = FaceModel.TriangleIndices;
            int triangleLength = triangleIndices.Count;

            return triangleLength;
        }

        public bool GetFaceModelTriangles(bool bMirrored, ref int[] avTriangles)
        {
            var triangleIndices = FaceModel.TriangleIndices;
            int triangleLength = triangleIndices.Count;

            if (avTriangles.Length >= triangleLength)
            {
                for (int i = 0; i < triangleLength; i += 3)
                {
                    //avTriangles[i] = (int)triangleIndices[i];
                    avTriangles[i] = (int)triangleIndices[i + 2];
                    avTriangles[i + 1] = (int)triangleIndices[i + 1];
                    avTriangles[i + 2] = (int)triangleIndices[i];
                }

                if (bMirrored)
                {
                    Array.Reverse(avTriangles);
                }

                return true;
            }

            return false;
        }

        public bool IsSpeechRecognitionAvailable(ref bool bNeedRestart)
        {
            bool bOneCopied = false, bAllCopied = true;

            if (!KinectInterop.Is64bitArchitecture())
            {
                //Debug.Log("Speech - x32-architecture.");
                string sTargetPath = KinectInterop.GetTargetDllPath(".", false) + "/";

                Dictionary<string, string> dictFilesToUnzip = new Dictionary<string, string>();
                dictFilesToUnzip["Kinect2SpeechWrapper.dll"] = sTargetPath + "Kinect2SpeechWrapper.dll";
                dictFilesToUnzip["msvcp110.dll"] = sTargetPath + "msvcp110.dll";
                dictFilesToUnzip["msvcr110.dll"] = sTargetPath + "msvcr110.dll";

                KinectInterop.UnzipResourceFiles(dictFilesToUnzip, "KinectV2UnityAddin.x86.zip", ref bOneCopied,
                    ref bAllCopied);
            }
            else
            {
                //Debug.Log("Face - x64-architecture.");
                string sTargetPath = KinectInterop.GetTargetDllPath(".", true) + "/";

                Dictionary<string, string> dictFilesToUnzip = new Dictionary<string, string>();
                dictFilesToUnzip["Kinect2SpeechWrapper.dll"] = sTargetPath + "Kinect2SpeechWrapper.dll";
                dictFilesToUnzip["msvcp110.dll"] = sTargetPath + "msvcp110.dll";
                dictFilesToUnzip["msvcr110.dll"] = sTargetPath + "msvcr110.dll";

                KinectInterop.UnzipResourceFiles(dictFilesToUnzip, "KinectV2UnityAddin.x64.zip", ref bOneCopied,
                    ref bAllCopied);
            }

            bNeedRestart = (bOneCopied && bAllCopied);

            return true;
        }

        public int InitSpeechRecognition(string sRecoCriteria, bool bUseKinect, bool bAdaptationOff)
        {
//		if(kinectSensor != null)
//		{
//			float fWaitTime = Time.realtimeSinceStartup + 5f;
//
//			while(!kinectSensor.IsAvailable && Time.realtimeSinceStartup < fWaitTime)
//			{
//				// wait
//			}
//		}

            return InitSpeechRecognizerNative(sRecoCriteria, bUseKinect, bAdaptationOff);
        }

        public void FinishSpeechRecognition()
        {
            FinishSpeechRecognizerNative();
        }

        public int UpdateSpeechRecognition()
        {
            return UpdateSpeechRecognizerNative();
        }

        public int LoadSpeechGrammar(string sFileName, short iLangCode, bool bDynamic)
        {
            return LoadSpeechGrammarNative(sFileName, iLangCode, bDynamic);

//		int hr = AddSpeechGrammarNative(sFileName, iLangCode, bDynamic);
//		if(hr >= 0)
//		{
//			hr = SetGrammarStateNative(sFileName, true);
//		}
//
//		return hr;
        }

        public int AddGrammarPhrase(string sFromRule, string sToRule, string sPhrase, bool bClearRulePhrases,
            bool bCommitGrammar)
        {
            return AddGrammarPhraseNative(sFromRule, sToRule, sPhrase, bClearRulePhrases, bCommitGrammar);
        }

        public void SetSpeechConfidence(float fConfidence)
        {
            SetSpeechConfidenceNative(fConfidence);
        }

        public bool IsSpeechStarted()
        {
            return IsSpeechStartedNative();
        }

        public bool IsSpeechEnded()
        {
            return IsSpeechEndedNative();
        }

        public bool IsPhraseRecognized()
        {
            return IsPhraseRecognizedNative();
        }

        public float GetPhraseConfidence()
        {
            return GetPhraseConfidenceNative();
        }

        public string GetRecognizedPhraseTag()
        {
            IntPtr pPhraseTag = GetRecognizedPhraseTagNative();
            string sPhraseTag = Marshal.PtrToStringUni(pPhraseTag);

            return sPhraseTag;
        }

        public void ClearRecognizedPhrase()
        {
            ClearRecognizedPhraseNative();
        }

        public bool IsBackgroundRemovalAvailable(ref bool bNeedRestart)
        {
            _bBackgroundRemovalInit = KinectInterop.IsOpenCvAvailable(ref bNeedRestart);
            return _bBackgroundRemovalInit;
        }

        public bool InitBackgroundRemoval(KinectInterop.SensorData sensorData, bool isHiResPrefered)
        {
            return KinectInterop.InitBackgroundRemoval(sensorData, isHiResPrefered);
        }

        public void FinishBackgroundRemoval(KinectInterop.SensorData sensorData)
        {
            KinectInterop.FinishBackgroundRemoval(sensorData);
            _bBackgroundRemovalInit = false;
        }

        public bool UpdateBackgroundRemoval(KinectInterop.SensorData sensorData, bool isHiResPrefered, Color32 defaultColor,
            bool bAlphaTexOnly)
        {
            return KinectInterop.UpdateBackgroundRemoval(sensorData, isHiResPrefered, defaultColor, bAlphaTexOnly);
        }

        public bool IsBackgroundRemovalActive()
        {
            return _bBackgroundRemovalInit;
        }

        public bool IsBRHiResSupported()
        {
            return true;
        }

        public Rect GetForegroundFrameRect(KinectInterop.SensorData sensorData, bool isHiResPrefered)
        {
            return KinectInterop.GetForegroundFrameRect(sensorData, isHiResPrefered);
        }

        public int GetForegroundFrameLength(KinectInterop.SensorData sensorData, bool isHiResPrefered)
        {
            return KinectInterop.GetForegroundFrameLength(sensorData, isHiResPrefered);
        }

        public bool PollForegroundFrame(KinectInterop.SensorData sensorData, bool isHiResPrefered, Color32 defaultColor,
            bool bLimitedUsers, ICollection<int> alTrackedIndexes, ref byte[] foregroundImage)
        {
            return KinectInterop.PollForegroundFrame(sensorData, isHiResPrefered, defaultColor, bLimitedUsers,
                alTrackedIndexes, ref foregroundImage);
        }

        public bool DeviceIsActive()
        {
            if (KinectSensor == null)
                return false;

            return KinectSensor.IsAvailable && KinectSensor.IsOpen;
        }
    }
}
#endif