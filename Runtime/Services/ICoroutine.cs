using UnityEngine;
using System.Collections;

namespace Agava.Wink
{
    public interface ICoroutine
    {
        Coroutine StartCoroutine(IEnumerator coroutine);
        void StopCoroutine(Coroutine coroutine);
    }
}
