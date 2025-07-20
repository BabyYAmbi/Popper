using System;
using UnityEngine;

namespace PopperBurst
{

    public interface IGameEventSystem
    {
        void Subscribe<T>(Action<T> callback) where T : IGameEvent;
        void Unsubscribe<T>(Action<T> callback) where T : IGameEvent;
        void Publish<T>(T gameEvent) where T : IGameEvent;
    }
}