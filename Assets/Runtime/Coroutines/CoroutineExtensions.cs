using System.Collections;
using UnityEngine;

namespace Runtime.Coroutines
{
    public static class CoroutineExtensions
    {
        public static CallbackCoroutine<T> WithResult<T>(this IEnumerator routine, MonoBehaviour coroutineRunner) =>
            new(coroutineRunner, routine);
    }
}