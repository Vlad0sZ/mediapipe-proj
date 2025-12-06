namespace Runtime.Infrastructure.Stacks
{
    public interface IStack<out T>
    {
        T GetNext();

        void Reset();
    }
}