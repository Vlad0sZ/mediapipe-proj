namespace SensorPack.KinectCore.Samples.UIhand
{
    public interface IRaycastObject
    {
        void OnCastEnter();
        void OnCastStay(float normalizedForce);
        void OnCastExit();
    }

    public interface ICastObject
    {
        InteractionType InteractionType { get; }
    }

    public enum InteractionType
    {
        /// <summary>
        /// Тип нажатия - Мышь
        /// </summary>
        Mouse = 1 << 1,
        
        /// <summary>
        /// Тип нажатия - Левая рука
        /// </summary>
        LeftHand = 1 << 2,
        
        /// <summary>
        /// Тип нажатия - Правая рука
        /// </summary>
        RightHand = 2 << 3,
        
        /// <summary>
        /// Тип нажатия - рука (любая)
        /// </summary>
        Hand = LeftHand | RightHand
    }
}