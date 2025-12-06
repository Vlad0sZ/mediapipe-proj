namespace Runtime.Game
{
    public struct ElapsedTime
    {
        public float Total;
        public float Progress;

        public ElapsedTime(float total, float progress)
        {
            Total = total;
            Progress = progress;
        }
    }
}