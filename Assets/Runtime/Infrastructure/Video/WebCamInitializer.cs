using System;
using System.Linq;
using UnityEngine;

namespace Runtime.Infrastructure.Video
{
    public class WebCamInitializer : IWebCamInitializer
    {
        public bool IsWebcamInitialized()
        {
            WebCamDevice[] devices = WebCamTexture.devices;

            UnityEngine.Debug.Log(
                $"get devices = {devices.Length}: \n {string.Join(" || ", devices.Select(x => x.name))}");

            if (devices.Length == 0)
                return false;

            if (devices.Length == 1 && devices[0].name.IndexOf("OBS", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            return true;
        }

        public WebCamDevice[] GetDevices() =>
            WebCamTexture.devices;
    }
}