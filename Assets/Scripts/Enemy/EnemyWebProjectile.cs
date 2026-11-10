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

        // Lưu mục tiêu lúc bắn. Đạn chỉ gây sát thương và tự hủy khi va đúng Player đã được chỉ định.
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

            // Đánh dấu đã trúng trước khi trừ máu vì Destroy sẽ dọn object vào cuối frame.
            spent = true;
            targetHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
