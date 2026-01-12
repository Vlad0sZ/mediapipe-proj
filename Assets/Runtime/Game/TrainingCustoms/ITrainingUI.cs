using Runtime.Game.Interfaces;

namespace Runtime.Game.TrainingCustoms
{
    public interface ITrainingUI
    {
        void ShowRaiseHandsHint();
        void HideRaiseHandsHint();

        void ShowCollectHintOnItem(ICollectableItem item, bool isPositive);

        void HideCollectHint(bool isPositive);

        void ShowScoreHint();
        void ShowTimerHint();

        void ShowFinishHint();

        void HideHints();
    }
}