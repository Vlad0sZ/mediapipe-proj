using System;
using System.Collections;
using UnityEngine;

namespace Runtime.Coroutines
{
    public class SomeMonoComponent : MonoBehaviour
    {
        private IEnumerator Start()
        {
            UnityEngine.Debug.Log("Start Coroutine");
            yield return new WaitForSeconds(2f);
            yield return InnerRoutine().WithResult<float>(this);

        }


        private IEnumerator InnerRoutine()
        {
            yield return new WaitForSeconds(2f);
            yield return 1f;
        }
    }
}