using System;
using UnityEngine;

namespace PopperBurst
{
    public interface IProjectileLauncher
    {
        void LaunchProjectiles(Vector3 position, Action<Collider2D> onHitCallback);
    }
}