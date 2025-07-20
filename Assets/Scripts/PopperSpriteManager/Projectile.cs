using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private PopperController sourcePopper; // Who fired this projectile
    private Vector2 direction;
    private float speed;
    private float lifetime;
    private Action<Collider2D> onHitCallback;
    private bool hasHit = false;

    public void Initialize(Vector2 dir, float spd, float life, Action<Collider2D> hitCallback, PopperController source = null)
    {
        direction = dir;
        speed = spd;
        lifetime = life;
        onHitCallback = hitCallback;
        sourcePopper = source; // Store who fired this

        Destroy(gameObject, lifetime);
    }


    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        PopperController popper = other.GetComponent<PopperController>();
        if (popper != null && popper != sourcePopper) // Ignore source popper!
        {
            hasHit = true;
            onHitCallback?.Invoke(other);
            Destroy(gameObject);
        }
    }
}