using PopperBurst;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameEventSystem : MonoBehaviour, IGameEventSystem
{
    private Dictionary<Type, List<Delegate>> eventCallbacks = new Dictionary<Type, List<Delegate>>();

    public void Subscribe<T>(Action<T> callback) where T : IGameEvent
    {
        Type eventType = typeof(T);

        if (!eventCallbacks.ContainsKey(eventType))
            eventCallbacks[eventType] = new List<Delegate>();

        eventCallbacks[eventType].Add(callback);
    }

    public void Unsubscribe<T>(Action<T> callback) where T : IGameEvent
    {
        Type eventType = typeof(T);

        if (eventCallbacks.ContainsKey(eventType))
        {
            eventCallbacks[eventType].Remove(callback);
        }
    }

    public void Publish<T>(T gameEvent) where T : IGameEvent
    {
        Type eventType = typeof(T);

        if (eventCallbacks.ContainsKey(eventType))
        {
            foreach (Delegate callback in eventCallbacks[eventType])
            {
                ((Action<T>)callback).Invoke(gameEvent);
            }
        }
    }
}