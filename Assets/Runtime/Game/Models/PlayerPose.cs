using Runtime.Types;

namespace Runtime.Game
{
    
    /// <summary>
    /// Структура данных для отслеживания жестов поднятия рук игрока.
    /// </summary>
    public readonly struct PlayerPose
    {
        /// <summary>
        /// Определяется ли игрок.
        /// </summary>
        public bool IsVisible { get; init; }

        /// <summary>
        /// Тип жеста <see cref="HandRaiseType"/>.
        /// </summary>
        public HandRaiseType HandRaiseType { get; init; }

        public PlayerPose(bool isVisible, HandRaiseType handRaiseType)
        {
            IsVisible = isVisible;
            HandRaiseType = handRaiseType;
        }
    }
}