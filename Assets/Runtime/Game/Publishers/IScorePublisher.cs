using R3;

namespace Runtime.Game.Publishers
{
    public interface IScorePublisher
    {
        ScoreModel Score { get; }
        
        Observable<ScoreModel> OnScore { get; }
    }
}