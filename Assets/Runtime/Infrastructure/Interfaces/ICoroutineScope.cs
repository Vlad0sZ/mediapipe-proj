using System.Collections;
using UnityEngine;

namespace Runtime.Infrastructure.Interfaces
{
    public interface ICoroutineScope
    {
        Coroutine StartCoroutine(IEnumerator routine);

        void StopCoroutine(Coroutine coroutine);
    }
}