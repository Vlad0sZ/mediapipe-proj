namespace Runtime.Machine
{
    public interface IStateMachine
    {
        public void ChangeState<T>() where T : IAsyncState;
        public void ChangeState(System.Type type);
    }
}