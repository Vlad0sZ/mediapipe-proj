using System;
using System.Collections.Generic;
using R3;
using SensorPack.KinectCore.Runtime;
using VContainer.Unity;

namespace Runtime.Game.Publishers
{
    public interface IPosePublisher : ITickable, IDisposable
    {
        public Observable<KinectInterop.BodyData> ActivePlayer { get; }
    }
}