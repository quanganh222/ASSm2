using UnityEngine;

namespace Metroidvania.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyChase))]
    public sealed class EnemyPatrol : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float leftDistance = 3f;
        [SerializeField, Min(0.1f)] private float rightDistance = 3f;
        [SerializeField, Min(0f)] private float patrolSpeed = 1.5f;
        [SerializeField, Min(0f)] private float waitAtEnd = 0.8f;

        private float startX;
        private bool movingRight = true;
        private float waitUntil;
        private bool initialized;

        private void Awake()
        {
            startX = transform.position.x;
            initialized = true;
        }

        // EnemyChase calls this only while no player is detected.
        public float GetHorizontalSpeed(float currentX)
        {
            if (Time.time < waitUntil) return 0f;

            float targetX = movingRight ? startX + rightDistance : startX - leftDistance;
            float delta = targetX - currentX;
            if (Mathf.Abs(delta) <= 0.05f)
            {
                movingRight = !movingRight;
                waitUntil = Time.time + waitAtEnd;
                return 0f;
            }

            return Mathf.Sign(delta) * Mathf.Min(patrolSpeed, Mathf.Abs(delta) / Time.fixedDeltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            float centerX = Application.isPlaying && initialized ? startX : transform.position.x;
            Vector3 left = new Vector3(centerX - leftDistance, transform.position.y, transform.position.z);
            Vector3 right = new Vector3(centerX + rightDistance, transform.position.y, transform.position.z);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(left, right);
            Gizmos.DrawWireSphere(left, 0.15f);
            Gizmos.DrawWireSphere(right, 0.15f);
        }
    }
}
