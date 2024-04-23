using System;

namespace Agava.Wink
{
    public interface IWinkSignInHandlerUI
    {
        bool IsAnyWindowEnabled { get; }

        event Action AllWindowsClosed;
        event Action SignInWindowClosed;

        void OpenSignWindow();
        void OpenWindow(WindowType type);
        void CloseWindow(WindowType type);
        void CloseAllWindows();
    }
}