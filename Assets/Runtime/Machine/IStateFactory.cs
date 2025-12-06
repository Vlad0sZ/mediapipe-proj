namespace Runtime.Machine
{
    public interface IStateFactory
    {
        TState CreateState<TState>() where TState : IState;
    }
}