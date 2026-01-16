namespace Runtime.Game.Models
{
    /// <summary>
    /// Структура для хранения рекорда пользователя.
    /// </summary>
    [System.Serializable]
    public struct UserRecord
    {
        /// <summary>
        /// Место в списке рекордсменов.
        /// </summary>
        public int place;
        
        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string userName;
        
        /// <summary>
        /// Количество очков.
        /// </summary>
        public int userScore;
    }
}