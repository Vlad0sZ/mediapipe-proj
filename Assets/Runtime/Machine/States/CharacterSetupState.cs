using JetBrains.Annotations;
using Runtime.UI.Interfaces;
using Runtime.UI.Screen;

namespace Runtime.Machine.States
{
    [UsedImplicitly]
    public sealed class CharacterSetupState : UIState
    {
        public CharacterSetupState(ICanvas canvas) : base(canvas, ScreenNames.Character)
        {
        }
    }
}