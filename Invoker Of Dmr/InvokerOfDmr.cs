using System;
using System.Collections;
using UnityEngine;

namespace Dmr.Utulities.Invokers
{
    public static class InvokerOfDmr
    {
        public static void InvokeWithDelay<T1, T2, T3, T4, T5, T6>(MonoBehaviour monoBehaviour, Action<T1, T2, T3, T4, T5, T6> action, float delay, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            monoBehaviour.StartCoroutine(InvokeCoroutine(action, delay, arg1, arg2, arg3, arg4, arg5, arg6));
        }

        public static void InvokeWithDelay<T1, T2, T3, T4, T5>(MonoBehaviour monoBehaviour, Action<T1, T2, T3, T4, T5> action, float delay, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            monoBehaviour.StartCoroutine(InvokeCoroutine(action, delay, arg1, arg2, arg3, arg4, arg5));
        }

        public static void InvokeWithDelay<T1, T2, T3, T4>(MonoBehaviour monoBehaviour, Action<T1, T2, T3, T4> action, float delay, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            monoBehaviour.StartCoroutine(InvokeCoroutine(action, delay, arg1, arg2, arg3, arg4));
        }

        public static void InvokeWithDelay<T1, T2, T3>(MonoBehaviour monoBehaviour, Action<T1, T2, T3> action, float delay, T1 arg1, T2 arg2, T3 arg3)
        {
            monoBehaviour.StartCoroutine(InvokeCoroutine(action, delay, arg1, arg2, arg3));
        }

        public static void InvokeWithDelay<T1, T2>(MonoBehaviour monoBehaviour, Action<T1, T2> action, float delay, T1 arg1, T2 arg2)
        {
            monoBehaviour.StartCoroutine(InvokeCoroutine(action, delay, arg1, arg2));
        }

        public static void InvokeWithDelay<T>(MonoBehaviour monoBehaviour, Action<T> action, float delay, T arg)
        {
            monoBehaviour.StartCoroutine(InvokeCoroutine(action, delay, arg));
        }

        public static void InvokeWithDelay(MonoBehaviour monoBehaviour, Action action, float delay)
        {
            monoBehaviour.StartCoroutine(InvokeCoroutine(action, delay));
        }
        private static IEnumerator InvokeCoroutine<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, float delay, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            yield return new WaitForSecondsRealtime(delay);
            action.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
        }

        private static IEnumerator InvokeCoroutine<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, float delay, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            yield return new WaitForSecondsRealtime(delay);
            action.Invoke(arg1, arg2, arg3, arg4, arg5);
        }

        private static IEnumerator InvokeCoroutine<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, float delay, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            yield return new WaitForSecondsRealtime(delay);
            action.Invoke(arg1, arg2, arg3, arg4);
        }

        private static IEnumerator InvokeCoroutine<T1, T2, T3>(Action<T1, T2, T3> action, float delay, T1 arg1, T2 arg2, T3 arg3)
        {
            yield return new WaitForSecondsRealtime(delay);
            action.Invoke(arg1, arg2, arg3);
        }

        private static IEnumerator InvokeCoroutine<T1, T2>(Action<T1, T2> action, float delay, T1 arg1, T2 arg2)
        {
            yield return new WaitForSecondsRealtime(delay);
            action.Invoke(arg1, arg2);
        }

        private static IEnumerator InvokeCoroutine<T>(Action<T> action, float delay, T arg)
        {
            yield return new WaitForSecondsRealtime(delay);
            action.Invoke(arg);
        }

        private static IEnumerator InvokeCoroutine(Action action, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            action.Invoke();
        }
    }
}
