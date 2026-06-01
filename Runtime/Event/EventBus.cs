using System;
using System.Collections.Generic;

namespace Air.UnityGameCore.Runtime.Event
{
    public interface IEventSubscription
    {
        int ListenersCount { get; }
    }

    sealed class EventSubscription : IEventSubscription
    {
        public Action Action;
        public int ListenersCount => Action?.GetInvocationList().Length ?? 0;
        public EventSubscription(Action action) => Action = action;
    }

    sealed class EventSubscription<T> : IEventSubscription
    {
        public Action<T> Action;
        public int ListenersCount => Action?.GetInvocationList().Length ?? 0;
        public EventSubscription(Action<T> action) => Action = action;
    }

    /// <summary>
    /// 字符串键的全局事件总线。
    /// </summary>
    public sealed class EventBus
    {
        readonly Dictionary<string, IEventSubscription> _events = new();

        public void On<T>(string eventName, Action<T> callback)
        {
            if (_events.TryGetValue(eventName, out var existing) && existing is EventSubscription<T> typed)
                typed.Action += callback;
            else
                _events[eventName] = new EventSubscription<T>(callback);
        }

        public void On(string eventName, Action callback)
        {
            if (_events.TryGetValue(eventName, out var existing) && existing is EventSubscription typed)
                typed.Action += callback;
            else
                _events[eventName] = new EventSubscription(callback);
        }

        public void Emit<T>(string eventName, T payload)
        {
            if (_events.TryGetValue(eventName, out var existing) && existing is EventSubscription<T> typed)
                typed.Action?.Invoke(payload);
        }

        public void Emit(string eventName)
        {
            if (_events.TryGetValue(eventName, out var existing) && existing is EventSubscription typed)
                typed.Action?.Invoke();
        }

        public void Off<T>(string eventName, Action<T> callback)
        {
            if (!_events.TryGetValue(eventName, out var existing) || existing is not EventSubscription<T> typed)
                return;
            typed.Action -= callback;
            if (typed.ListenersCount == 0)
                _events.Remove(eventName);
        }

        public void Off(string eventName, Action callback)
        {
            if (!_events.TryGetValue(eventName, out var existing) || existing is not EventSubscription typed)
                return;
            typed.Action -= callback;
            if (typed.ListenersCount == 0)
                _events.Remove(eventName);
        }

        public void Remove(string eventName) => _events.Remove(eventName);

        public void Clear() => _events.Clear();
    }
}
