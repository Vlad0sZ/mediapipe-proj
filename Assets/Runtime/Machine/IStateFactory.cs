namespace Runtime.Machine
{
    public interface IStateFactory
    {
        IAsyncState CreateState(System.Type stateType);
    }
}