using UnityEngine;

namespace Runtime.Game.Interfaces
{
    public interface IGameObject
    {
        GameObject gameObject { get; }
        
        bool IsDisposed { get; }
    }
}