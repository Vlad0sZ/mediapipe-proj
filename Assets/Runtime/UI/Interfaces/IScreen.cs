
using System;

namespace Runtime.UI.Interfaces
{
    public interface IScreen
    {
        void Show(bool instantly = false, Action callback = null);

        void Hide(bool instantly = false,  Action callback = null);
    }

}