using System;
using System.Collections;
using UnityEngine;


namespace Dmr.Utulities.Invokers
{
    public static class WaitUntilOfDmr
    {
        public static void WaitUntil<T1, T2, T3, T4, T5>(
            MonoBehaviour monoBehaviour, Action<T1, T2, T3, T4, T5> action,
            Func<bool> predicate, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            monoBehaviour.StartCoroutine(InvokeCoroutine(action, predicate, arg1, arg2, arg3, arg4, arg5));
        }

        public static void WaitUntil<T1, T2, T3, T4>(
            MonoBehaviour monoBehaviour, Action<T1, T2, T3, T4> action,
            Func<bool> predicate, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            monoBehaviour.StartCoroutine(InvokeCoroutine(action, predicate, arg1, arg2, arg3, arg4));
        }

        public static void WaitUntil<T1, T2, T3>(
            MonoBehaviour monoBehaviour, Action<T1, T2, T3> action,
            Func<bool> predicate, T1 arg1, T2 arg2, T3 arg3)
        {
            monoBehaviour.StartCoroutine(InvokeCoroutine(action, predicate, arg1, arg2, arg3));
        }

        public static void WaitUntil<T1, T2>(
            MonoBehaviour monoBehaviour, Action<T1, T2> action,
            Func<bool> predicate, T1 arg1, T2 arg2)
        {
            monoBehaviour.StartCoroutine(InvokeCoroutine(action, predicate, arg1, arg2));
        }
        public static void WaitUntil<T1>(
        MonoBehaviour monoBehaviour, Action<T1> action,
        Func<bool> predicate, T1 arg1)
        {
            monoBehaviour.StartCoroutine(InvokeCoroutine(action, predicate, arg1));
        }
        public static void WaitUntil(
            MonoBehaviour monoBehaviour, Action action,
            Func<bool> predicate)
        {
            monoBehaviour.StartCoroutine(InvokeCoroutine(action, predicate));
        }

        private static IEnumerator InvokeCoroutine<T1, T2, T3, T4, T5>(
            Action<T1, T2, T3, T4, T5> action, Func<bool> predicate,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            yield return new WaitUntil(predicate);
            action.Invoke(arg1, arg2, arg3, arg4, arg5);
        }

        private static IEnumerator InvokeCoroutine<T1, T2, T3, T4>(
            Action<T1, T2, T3, T4> action, Func<bool> predicate,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            yield return new WaitUntil(predicate);
            action.Invoke(arg1, arg2, arg3, arg4);
        }

        private static IEnumerator InvokeCoroutine<T1, T2, T3>(
            Action<T1, T2, T3> action, Func<bool> predicate,
            T1 arg1, T2 arg2, T3 arg3)
        {
            yield return new WaitUntil(predicate);
            action.Invoke(arg1, arg2, arg3);
        }

        private static IEnumerator InvokeCoroutine<T1, T2>(
            Action<T1, T2> action, Func<bool> predicate,
            T1 arg1, T2 arg2)
        {
            yield return new WaitUntil(predicate);
            action.Invoke(arg1, arg2);
        }
        private static IEnumerator InvokeCoroutine<T1>(
           Action<T1> action, Func<bool> predicate,
           T1 arg1)
        {
            yield return new WaitUntil(predicate);
            action.Invoke(arg1);
        }
        private static IEnumerator InvokeCoroutine(
            Action action, Func<bool> predicate)
        {
            yield return new WaitUntil(predicate);
            action.Invoke();
        }
    }
}
