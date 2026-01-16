namespace Runtime.Types
{
    /// <summary>
    /// Тип поднятния рук у игрока.
    /// </summary>
    [System.Flags]
    public enum HandRaiseType
    {
        /// <summary>
        /// Неопределено.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Левая рука поднята.
        /// </summary>
        LeftHandRaised = 1 << 0,
        
        /// <summary>
        /// Правая рука поднята.
        /// </summary>
        RightHandRaised = 1 << 2,
        
        /// <summary>
        /// Левая рука опущена.
        /// </summary>
        LeftHandBelow = 1 << 3,
        
        /// <summary>
        /// Правая рука опущена.
        /// </summary>
        RightHandBelow = 1 << 4,


        /// <summary>
        /// Обе руки подняты.
        /// </summary>
        HandsRaised = LeftHandRaised | RightHandRaised,
        
        /// <summary>
        /// Обе руки опущены.
        /// </summary>
        HandsBelow = LeftHandBelow | RightHandBelow,
    }
}