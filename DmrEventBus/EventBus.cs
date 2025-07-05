using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DmrEventBus
{
    public static class EventBus
    {
        private static readonly ConcurrentDictionary<WeakReference<object>, ConcurrentDictionary<(Delegate originalMethod, Action<IEvent> wrappedMethod), byte>> _subscribedClassToEvents
            = new ConcurrentDictionary<WeakReference<object>, ConcurrentDictionary<(Delegate, Action<IEvent>), byte>>(new WeakRefComparer());

        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<Action<IEvent>, byte>> _guidToEventDelegates = new();
        private static readonly ConcurrentDictionary<Type, string> _eventDict = new();

        public static volatile bool _CleanupFlag = false;
        private static readonly ConcurrentBag<WeakReference<object>> _deadObjects = new ConcurrentBag<WeakReference<object>>();

        // New: secondary index for O(1) Unsubscribe
        private static readonly ConcurrentDictionary<Delegate, (WeakReference<object> weakSender, Action<IEvent> wrappedMethod, string guid)>
            _actionIndex = new ConcurrentDictionary<Delegate, (WeakReference<object>, Action<IEvent>, string)>();

        // Secondary: index sender+eventGuid to set of original methods for O(1) Unsubscribe<T>(sender)
        private static readonly ConcurrentDictionary<(WeakReference<object> weakSender, string guid), ConcurrentDictionary<Delegate, byte>>
            _senderTypeIndex = new ConcurrentDictionary<(WeakReference<object>, string), ConcurrentDictionary<Delegate, byte>>();

        // O(1) FindWeakRef by sender
        private static readonly ConcurrentDictionary<object, WeakReference<object>> _senderToWeakRef = new();

        private static readonly ReaderWriterLockSlim _rw = new ();

        public static void Subscribe<T>(object sender, Action<T> action) where T : IEvent
        {
            _rw.EnterWriteLock();

            try
            {
                if (sender is not MonoBehaviour && sender is not IDisposable && sender is not ScriptableObject)
                {
                    Debug.LogWarning($"EventBus.Subscribe<T>(): No event is registered for type {typeof(T).Name} by {sender.GetType().Name} because it is not a MonoBehaviour, ScriptableObject or a IDisposable.");
                    return;
                }

                // Create or find weak reference for sender
                var weakSender = FindOrCreateWeakRef(sender);
                var subscribedEvents = _subscribedClassToEvents.GetOrAdd(weakSender, _ => new ConcurrentDictionary<(Delegate, Action<IEvent>), byte>());

                if (subscribedEvents.Keys.Any(t => t.originalMethod == (Delegate)action))
                {
                    Debug.LogWarning($"EventBus.Subscribe<T>(): Event {typeof(T).Name} is already subscribed to by {sender.GetType().Name}.");
                    return;
                }

                string guid = GetOrCreateEventGuid<T>();
                var eventDelegates = GetOrCreateEventDelegates(guid);

                // Capture only the weak reference inside the lambda to avoid strong closure
                Action<IEvent> wrappedMethod = (IEvent e) =>
                {
                    if (!weakSender.TryGetTarget(out var target) || IsUnityObjectDead(target))
                    {
                        _CleanupFlag = true;
                        _deadObjects.Add(weakSender);
                        return;
                    }
                    action((T)e);
                };

                if (!eventDelegates.TryAdd(wrappedMethod, 0))
                {
                    Debug.LogError($"EventBus.Subscribe<T>(): unable to add type {typeof(T).Name} to eventDelegates.");
                    return;
                }

                subscribedEvents.TryAdd((action, wrappedMethod), 0);

                _actionIndex.TryAdd(action, (weakSender, wrappedMethod, guid));

                var key = (weakSender, guid);
                var methodSet = _senderTypeIndex.GetOrAdd(key, _ => new ConcurrentDictionary<Delegate, byte>());
                methodSet.TryAdd(action, 0);
            }
            finally { _rw.ExitWriteLock(); }
        }

        public static void Unsubscribe<T>(object sender) where T : IEvent
        {
            var weakSender = FindWeakRef(sender);
            if (weakSender == null) return;

            if (!_eventDict.TryGetValue(typeof(T), out var guid)) return;
            var key = (weakSender, guid);

            if (_senderTypeIndex.TryRemove(key, out var methodSet))
            {
                foreach (var action in methodSet.Keys)
                {
                    if (_actionIndex.TryRemove(action, out var entry))
                    {
                        var (_, wrappedMethod, _) = entry;
                        if (_subscribedClassToEvents.TryGetValue(weakSender, out var subscribedEvents))
                            subscribedEvents.TryRemove((action, wrappedMethod), out _);

                        if (_guidToEventDelegates.TryGetValue(guid, out var eventDelegates))
                            eventDelegates.TryRemove(wrappedMethod, out _);
                    }
                }

                if (_subscribedClassToEvents.TryGetValue(weakSender, out var evs))
                    CleanupWeakRefIfEmpty(weakSender, evs, guid,typeof(T));
            }
        }

        public static void Unsubscribe<T>(object sender, Action<T> action) where T : IEvent
        {
            if (_actionIndex.TryRemove(action, out var entry))
            {
                var (weakSender, wrappedMethod, guid) = entry;
                if (_subscribedClassToEvents.TryGetValue(weakSender, out var subscribedEvents))
                    subscribedEvents.TryRemove((action, wrappedMethod), out _);

                if (_guidToEventDelegates.TryGetValue(guid, out var eventDelegates))
                    eventDelegates.TryRemove(wrappedMethod, out _);

                var key = (weakSender, guid);
                if (_senderTypeIndex.TryGetValue(key, out var methodSet))
                    methodSet.TryRemove(action, out _);

                CleanupWeakRefIfEmpty(weakSender, subscribedEvents, guid,typeof(T));
            }
            else
            {
                Debug.LogWarning($"EventBus.Unsubscribe<{typeof(T).Name}>(): no subscription found for {sender.GetType().Name}.");
            }
        }
        public static void Publish<T>(T @event) where T : IEvent
        {
            // 1) Clean up once if flagged
            if (_CleanupFlag)
                CleanupDeadEvents();

            // 2) Bail early if no subscribers
            if (!_eventDict.TryGetValue(typeof(T), out var guid) ||
                !_guidToEventDelegates.TryGetValue(guid, out var eventDelegates))
            {
                return;
            }

            _rw.EnterReadLock();
            try
            {

                // 3) Iterate without allocations
                foreach (var handler in eventDelegates.Keys)
                {
                    if (handler == null)
                        continue;

                    try
                    {
                        handler(@event);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(
                            $"EventBus.Publish<{typeof(T).Name}>: handler threw exception:\n{ex}"
                        );
                        // leave cleanup flag alone—no extra allocations here
                    }
                }
            }
            finally { _rw.ExitReadLock(); }
        }

        #region Helpers
        private static WeakReference<object> FindOrCreateWeakRef(object sender)
        {
            if (_senderToWeakRef.TryGetValue(sender, out var existing))
                return existing;

            var weak = new WeakReference<object>(sender);
            _senderToWeakRef[sender] = weak;
            return weak;
        }

        private static WeakReference<object> FindWeakRef(object sender)
        {
            _senderToWeakRef.TryGetValue(sender, out var weak);
            return weak;
        }

        private static void CleanupWeakRefIfEmpty(WeakReference<object> weakSender,
            ConcurrentDictionary<(Delegate, Action<IEvent>), byte> events, string guid, Type eventType)
        {
            if (events.IsEmpty)
            {
                _subscribedClassToEvents.TryRemove(weakSender, out _);

                // Remove from sender index
                _senderTypeIndex.Keys.Where(k => k.weakSender == weakSender).ToList()
                    .ForEach(k => _senderTypeIndex.TryRemove(k, out _));

                if (_guidToEventDelegates.TryGetValue(guid, out var ds) && ds.IsEmpty)
                {
                    _guidToEventDelegates.TryRemove(guid, out _);
                    _eventDict.TryRemove(eventType, out _);
                }

                if (weakSender.TryGetTarget(out var target))
                {
                    _senderToWeakRef.TryRemove(target, out _);
                }
            }
        }

        private static string GetOrCreateEventGuid<T>() where T : IEvent
        {
            if (_eventDict.TryGetValue(typeof(T), out var existingGuid))
                return existingGuid;
            var guid = Guid.NewGuid().ToString();
            while (_guidToEventDelegates.ContainsKey(guid))
                guid = Guid.NewGuid().ToString();
            _eventDict.TryAdd(typeof(T), guid);
            return guid;
        }

        private static ConcurrentDictionary<Action<IEvent>, byte> GetOrCreateEventDelegates(string guid)
        {
            return _guidToEventDelegates.GetOrAdd(guid, _ => new ConcurrentDictionary<Action<IEvent>, byte>());
        }

        private static bool IsUnityObjectDead(object obj)
        {
            if (obj == null) return true;
            if (obj is UnityEngine.Object u) return u == null;
            return false;
        }

        private static void CleanupDeadEvents()
        {
            _CleanupFlag = false;
            while (_deadObjects.TryTake(out var wr))
            {
                if (wr.TryGetTarget(out var dead))
                    RemoveDeadObject(wr, dead);
                else
                    _subscribedClassToEvents.TryRemove(wr, out _);
            }
        }

        private static void RemoveDeadObject(WeakReference<object> wr, object dead)
        {
            if (!_subscribedClassToEvents.TryRemove(wr, out var tuples)) return;

            _senderTypeIndex.Keys.Where(k => k.weakSender == wr).ToList()
                .ForEach(k => _senderTypeIndex.TryRemove(k, out _));

            if (tuples == null) return;
            foreach (var tuple in tuples.Keys)
            {
                var eventType = tuple.originalMethod.GetType().GetGenericArguments()[0];
                if (_eventDict.TryGetValue(eventType, out var guid)
                    && _guidToEventDelegates.TryGetValue(guid, out var delegates))
                {
                    delegates.TryRemove(tuple.wrappedMethod, out _);
                    if (delegates.IsEmpty)
                    {
                        _guidToEventDelegates.TryRemove(guid, out _);
                        _eventDict.TryRemove(eventType, out _);
                    }
                }

                _actionIndex.TryRemove(tuple.originalMethod, out _);
            }

            if (dead != null)
            {
                _senderToWeakRef.TryRemove(dead, out _);
            }
        }

        private class WeakRefComparer : IEqualityComparer<WeakReference<object>>
        {
            public bool Equals(WeakReference<object> x, WeakReference<object> y)
            {
                bool gotX = x.TryGetTarget(out var tx);
                bool gotY = y.TryGetTarget(out var ty);

                if (!gotX && !gotY)
                    return true; // Both dead - treat as equal to avoid weird behavior

                if (gotX != gotY)
                    return false; // One dead, one alive

                return ReferenceEquals(tx, ty);
            }

            public int GetHashCode(WeakReference<object> obj)
            {
                if (obj.TryGetTarget(out var target))
                    return target.GetHashCode();
                return 0;
            }
        }
        #endregion
    }
}
