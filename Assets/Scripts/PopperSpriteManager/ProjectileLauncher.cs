using PopperBurst;
using System;
using UnityEngine;

public class ProjectileLauncher : MonoBehaviour, IProjectileLauncher
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileLifetime = 2f;

    private readonly Vector2[] directions = {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right
    };

    public void LaunchProjectiles(Vector3 position, Action<Collider2D> onHitCallback)
    {
        PopperController sourcePopper = GetComponent<PopperController>();
        foreach (Vector2 direction in directions)
        {
            LaunchSingleProjectile(position, direction, onHitCallback, sourcePopper);
        }
    }

    private void LaunchSingleProjectile(Vector3 startPos, Vector2 direction, Action<Collider2D> onHitCallback, PopperController source)
    {
        if (!projectilePrefab) return;

        GameObject projectile = Instantiate(projectilePrefab, startPos, Quaternion.identity);
        Projectile projScript = projectile.GetComponent<Projectile>();

        if (projScript == null)
            projScript = projectile.AddComponent<Projectile>();

        projScript.Initialize(direction, projectileSpeed, projectileLifetime, onHitCallback, source);
    }
}