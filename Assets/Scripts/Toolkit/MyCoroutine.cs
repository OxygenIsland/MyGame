using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarWorld.Common.Utility
{
    public class MyCoroutine : MonoBehaviour
    {
        public Coroutine RunSequence(List<IEnumerator> coroutines)
        {
            return StartCoroutine(Sequence(coroutines));
        }

        public void RunParallel(List<IEnumerator> coroutines)
        {
            StartCoroutine(Parallel(coroutines));
        }

        private IEnumerator Sequence(List<IEnumerator> coroutines)
        {
            foreach (var item in coroutines)
            {
                yield return item;
            }

            GameObject.Destroy(gameObject);
        }

        private IEnumerator Parallel(List<IEnumerator> coroutines)
        {
            foreach (var item in coroutines)
            {
                StartCoroutine(item);
            }

            yield return 0;
        }
    }
}