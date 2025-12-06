namespace Runtime.UI.Interfaces
{
    public interface IScreen
    {
        void Show(bool instantly = false);

        void Hide(bool instantly = false);
    }
}