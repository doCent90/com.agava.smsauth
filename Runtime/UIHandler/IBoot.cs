using System;

namespace Agava.Wink
{
    public interface IBoot
    {
        event Action Restarted;
    }
}