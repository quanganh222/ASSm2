using UnityEngine;

namespace Metroidvania.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyKnockback : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float pushSpeed = 5f;
        [SerializeField, Min(0.01f)] private float duration = 0.18f;
        [SerializeField] private bool flyingEnemy;

        private Rigidbody2D body;
        private float finishTime;
        private bool active;

        public bool IsActive => active;

        private void Awake() => body = GetComponent<Rigidbody2D>();

        public void PushAwayFrom(Vector2 attackerPosition)
        {
            Vector2 direction = body.position - attackerPosition;
            if (direction.sqrMagnitude < 0.0001f)
                direction = Vector2.right;

            if (flyingEnemy)
                body.linearVelocity = direction.normalized * pushSpeed;
            else
                body.linearVelocity = new Vector2(
                    (direction.x < 0f ? -1f : 1f) * pushSpeed,
                    body.linearVelocity.y);

            active = true;
            finishTime = Time.time + duration;
        }

        private void FixedUpdate()
        {
            if (active && Time.time >= finishTime)
                StopKnockback();
        }

        private void OnDisable()
        {
            if (body != null && active)
                StopKnockback();
        }

        private void StopKnockback()
        {
            active = false;
            body.linearVelocity = flyingEnemy
                ? Vector2.zero
                : new Vector2(0f, body.linearVelocity.y);
        }
    }
}
