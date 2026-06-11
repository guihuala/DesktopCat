using System;
using System.Collections.Generic;
using UnityEngine;

namespace DesktopPet.Events
{
    public static class GameEventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> Subscribers = new Dictionary<Type, List<Delegate>>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad()
        {
            Clear();
        }

        public static IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IGameEvent
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var eventType = typeof(TEvent);
            if (!Subscribers.TryGetValue(eventType, out var handlers))
            {
                handlers = new List<Delegate>();
                Subscribers[eventType] = handlers;
            }

            handlers.Add(handler);
            return new GameEventSubscription(() => Unsubscribe(handler));
        }

        public static void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IGameEvent
        {
            if (handler == null)
            {
                return;
            }

            var eventType = typeof(TEvent);
            if (!Subscribers.TryGetValue(eventType, out var handlers))
            {
                return;
            }

            handlers.Remove(handler);
            if (handlers.Count == 0)
            {
                Subscribers.Remove(eventType);
            }
        }

        public static void Publish<TEvent>(TEvent gameEvent) where TEvent : struct, IGameEvent
        {
            var eventType = typeof(TEvent);
            if (!Subscribers.TryGetValue(eventType, out var handlers) || handlers.Count == 0)
            {
                return;
            }

            var snapshot = ListPool<Delegate>.Get();
            snapshot.AddRange(handlers);

            for (var i = 0; i < snapshot.Count; i++)
            {
                if (snapshot[i] is Action<TEvent> handler)
                {
                    try
                    {
                        handler.Invoke(gameEvent);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            ListPool<Delegate>.Release(snapshot);
        }

        public static void Clear()
        {
            Subscribers.Clear();
        }

        private static class ListPool<T>
        {
            private static readonly Stack<List<T>> Pool = new Stack<List<T>>();

            public static List<T> Get()
            {
                return Pool.Count > 0 ? Pool.Pop() : new List<T>();
            }

            public static void Release(List<T> list)
            {
                list.Clear();
                Pool.Push(list);
            }
        }
    }
}
