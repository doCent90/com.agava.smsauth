using System;
using System.Collections.Generic;

namespace Agava.Wink
{
    public interface IWinkAccessManager
    {
        bool HasAccess { get; }

        event Action Successfully;
        event Action ResetLogin;
        event Action<IReadOnlyList<string>> LimitReached;
    }
}