using SensorPack.KinectCore.Runtime.Interfaces;
using UnityEngine;

namespace SensorPack.KinectCore.Runtime.Managers
{
    public abstract class SensorInterfacesHolder : MonoBehaviour
    {
        public abstract DepthSensorInterface[] GetAvailableInterfaces();
    }
}