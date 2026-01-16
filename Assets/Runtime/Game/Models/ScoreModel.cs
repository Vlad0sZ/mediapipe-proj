using UnityEngine;

namespace Runtime.Game
{
    /// <summary>
    /// Структура данных для набранных очков во время игры.
    /// </summary>
    public readonly struct ScoreModel
    {
        /// <summary>
        /// Набранные очки, собранные с правильных объектов.
        /// </summary>
        public readonly int PositiveScore;
        
        /// <summary>
        /// Набранные очки, собранные с неправильных объектов.
        /// </summary>
        public readonly int NegativeScore;

        public ScoreModel(int positiveScore, int negativeScore)
        {
            PositiveScore = positiveScore;
            NegativeScore = negativeScore;
        }

        /// <summary>
        /// Процентное соотношение очков, от -1 до 1.
        /// </summary>
        public float Progress()
        {
            var negative = Mathf.Abs(NegativeScore);
            var positive = PositiveScore;
            var totalScore = positive + negative;

            if (totalScore == 0)
                return 0f;

            return (positive - negative) / (float) (positive + negative);
        }
    }
}