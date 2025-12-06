using Runtime.Types;

namespace Runtime.Game
{
    public readonly struct PlayerPose
    {
        public bool IsVisible { get; init; }

        public HandRaiseType HandRaiseType { get; init; }

        public PlayerPose(bool isVisible, HandRaiseType handRaiseType)
        {
            IsVisible = isVisible;
            HandRaiseType = handRaiseType;
        }
    }
}