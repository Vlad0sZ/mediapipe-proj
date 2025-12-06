using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace Runtime.Coroutines
{
    public class ValueSetter<T>
    {
        private T _value;

        public void Set(T value) => _value = value;

        public T Get() => _value;
    }

    public class CallbackCoroutine<T> : CustomYieldInstruction
    {
        public CallbackCoroutine(MonoBehaviour owner, IEnumerator innerRoutine, long timeoutMs = long.MaxValue)
        {
            _coroutineRunner = owner;
            _innerRoutine = innerRoutine;
            _coroutine = _coroutineRunner.StartCoroutine(WaitForRoutine(timeoutMs));
        }

        private readonly MonoBehaviour _coroutineRunner;
        private readonly IEnumerator _innerRoutine;
        private readonly Coroutine _coroutine;
        private readonly long _timeout;
        private event Action<T> Callback;

        public T Result { get; private set; }

        public Exception Error { get; private set; }

        public bool IsError => Error != null;
        public bool IsCompleted { get; private set; }

        public override bool keepWaiting => IsCompleted == false;

        public void Subscribe(Action<T> onCompleted)
        {
            if (IsCompleted)
                onCompleted?.Invoke(Result);
            else
                Callback += onCompleted;
        }

        private IEnumerator WaitForRoutine(long timeout)
        {
            IsCompleted = false;
            var sw = Stopwatch.StartNew();
            object tmpResult = new();

            while (true)
            {
                try
                {
                    if (sw.ElapsedMilliseconds > timeout)
                    {
                        _coroutineRunner.StopCoroutine(_coroutine);
                        throw new TimeoutException($"{sw.ElapsedMilliseconds}ms has passed");
                    }

                    if (!_innerRoutine.MoveNext())
                        break;

                    tmpResult = _innerRoutine.Current;
                }
                catch (Exception e)
                {
                    Error = e;
                    yield break;
                }

                yield return tmpResult;
            }

            Complete(tmpResult);
        }

        private void Complete(object result)
        {
            if (result is T tRes)
                Result = tRes;

            Callback?.Invoke(Result);
            UnityEngine.Debug.Log($"Result = {result}");
            IsCompleted = true;
        }
    }
}