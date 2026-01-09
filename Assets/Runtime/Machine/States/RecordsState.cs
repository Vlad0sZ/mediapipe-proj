using Runtime.UI.Interfaces;
using Runtime.UI.Screen;

namespace Runtime.Machine.States
{
    public class RecordsState : UIState
    {
        public RecordsState(ICanvas canvas) : base(canvas, ScreenNames.Records)
        {
        }
    }
}