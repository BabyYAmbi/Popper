using UnityEngine;

namespace PopperBurst
{
    public struct PopperBurstEvent : IGameEvent
    {
        public PopperController popper;
        public Vector3 position;
    }
}