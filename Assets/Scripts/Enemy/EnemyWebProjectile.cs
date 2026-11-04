using UnityEngine;

namespace Metroidvania.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public sealed class EnemyWebProjectile : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float speed = 7f;
        [SerializeField, Min(0.1f)] private float lifetime = 2f;
        [SerializeField, Min(1)] private int damage = 10;

        private PlayerHealth targetHealth;
        private bool spent;

        public void Launch(Vector2 direction, Transform target)
        {
            targetHealth = target.GetComponent<PlayerHealth>();
            GetComponent<Rigidbody2D>().linearVelocity = direction.normalized * speed;
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryHitPlayer(other);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryHitPlayer(collision.collider);
        }

        private void TryHitPlayer(Collider2D other)
        {
            if (spent || targetHealth == null ||
                other.GetComponentInParent<PlayerHealth>() != targetHealth)
                return;

            spent = true;
            targetHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
