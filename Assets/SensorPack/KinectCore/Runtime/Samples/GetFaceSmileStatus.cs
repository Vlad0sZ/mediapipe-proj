#if (UNITY_STANDALONE_WIN)
using SensorPack.KinectCore.Runtime.Interfaces;
using UnityEngine;

namespace SensorPack.KinectCore.Runtime.Samples
{
	public class GetFaceSmileStatus : MonoBehaviour 
	{

		[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
		public int playerIndex = 0;

		[Tooltip("UI-Text to display the FT-manager debug messages.")]
		public UnityEngine.UI.Text debugText;

		[Tooltip("Currently detected smile status.")]
		public Windows.Kinect.DetectionResult smileStatus = Windows.Kinect.DetectionResult.Unknown;


		private KinectManager kinectManager;
		private KinectInterop.SensorData sensorData;


		void Start () 
		{
			kinectManager = KinectManager.Instance;

			if (kinectManager != null) 
			{
				sensorData = kinectManager.GetSensorData ();
			}
		}
	

		void Update () 
		{
			if (kinectManager == null || sensorData == null || sensorData.sensorInterface == null)
				return;

			long userId = kinectManager.GetUserIdByIndex (playerIndex);
			Kinect2Interface k2int = (Kinect2Interface)sensorData.sensorInterface;

			for (int i = 0; i < sensorData.bodyCount; i++)
			{
				if(k2int.FaceFrameSources != null && k2int.FaceFrameSources[i] != null && k2int.FaceFrameSources[i].TrackingId == (ulong)userId)
				{
					if(k2int.FaceFrameResults != null && k2int.FaceFrameResults[i] != null)
					{
						Windows.Kinect.DetectionResult newStatus = k2int.FaceFrameResults [i].FaceProperties [Microsoft.Kinect.Face.FaceProperty.Happy];

						if (newStatus != Windows.Kinect.DetectionResult.Unknown) 
						{
							smileStatus = newStatus;
						}

						debugText.text = "Smile-status: " + smileStatus;
					}
				}
			}
	
		}

	}
}
#endif
