using System.Threading.Tasks;
using JetBrains.Annotations;
using Runtime.UI.Interfaces;
using Runtime.UI.Screen;

namespace Runtime.Machine.States
{
    [UsedImplicitly]
    public sealed class MainMenuState : UIState
    {
        public MainMenuState(ICanvas canvas) : base(canvas, ScreenNames.MainMenu)
        {
        }
    }
}