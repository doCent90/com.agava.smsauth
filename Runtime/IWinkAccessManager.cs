using System;

namespace Agava.Wink
{
    public interface IWinkAccessManager
    {
        bool HasAccess { get; }

        event Action Successfully;
    }
}