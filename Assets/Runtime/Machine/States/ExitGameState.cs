using System.Collections;
using Runtime.Infrastructure.Interfaces;
using UnityEngine;

namespace Runtime.Machine.States
{
    public class ExitGameState : IState
    {
        private readonly ICoroutineScope _scope;
        public ExitGameState(ICoroutineScope scope) => _scope = scope;

        public void Activate() => _scope.StartCoroutine(ExitAfter());

        private static IEnumerator ExitAfter()
        {
            yield return new WaitForSeconds(0.24f);
            Application.Quit();
        }

        public void Deactivate()
        {
        }
    }
}