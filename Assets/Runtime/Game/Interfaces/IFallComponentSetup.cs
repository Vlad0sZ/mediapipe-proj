using Runtime.Game.ScriptableData;
using UnityEngine;

namespace Runtime.Game.Interfaces
{
    public interface IFallComponentSetup : ISetupPayload<GameSettings.ObjectsSettings>
    {
        void StopMove();

        void ContinueMove();
        
        void SetFallSpeed(float value);
        void SetRotationSpeed(float value);
        void SetRotationAxis(Vector3 axis);
    }
}