using System;

namespace Agava.Wink
{
    public interface IWinkSignInHandlerUI
    {
        bool IsAnyWindowEnabled { get; }

        event Action WindowsClosed;

        void OpenSignWindow();
        void OpenWindow(WindowPresenter window);
        void CloseWindow(WindowPresenter window);
        void CloseAllWindows();
    }
}