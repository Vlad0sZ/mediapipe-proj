using UnityEngine;

namespace Runtime.Infrastructure.Video
{
    public interface IWebCamInitializer
    {
        bool IsWebcamInitialized();

        WebCamDevice[] GetDevices();
    }
}