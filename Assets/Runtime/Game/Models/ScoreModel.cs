using UnityEngine;

namespace Runtime.Game
{
    public readonly struct ScoreModel
    {
        public readonly int PositiveScore;
        public readonly int NegativeScore;

        public ScoreModel(int positiveScore, int negativeScore)
        {
            PositiveScore = positiveScore;
            NegativeScore = negativeScore;
        }


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