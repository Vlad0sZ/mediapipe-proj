using SensorPack.Addons.Mediapipe;
using SensorPack.KinectCore.Runtime.Interfaces;
using SensorPack.KinectCore.Runtime.Managers;

namespace Runtime
{
    internal sealed class SensorDevices : SensorInterfacesHolder
    {
        public override DepthSensorInterface[] GetAvailableInterfaces() =>
            new DepthSensorInterface[]
            {
                new WebcamInterface(),
                new DummyK2Interface()
            };
    }
}