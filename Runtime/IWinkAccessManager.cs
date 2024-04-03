using System;

namespace Agava.Wink
{
    public interface IWinkAccessManager
    {
        bool HasAccess { get; }
        SaveLoadService SaveLoadService { get; }

        event Action Successfully;
    }
}