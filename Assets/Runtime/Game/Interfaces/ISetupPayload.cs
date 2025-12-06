namespace Runtime.Game.Interfaces
{
    public interface ISetupPayload<in TPayload>
    {
        void Setup(TPayload payload);
    }
}