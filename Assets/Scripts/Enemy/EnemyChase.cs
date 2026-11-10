using UnityEngine;

namespace Metroidvania.Gameplay
{
    // Quái mặt đất đuổi Player theo trục ngang khi vào tầm phát hiện và dừng ở khoảng cách đã cấu hình.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyChase : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField, Min(0f)] private float detectionRange = 5f;
        [SerializeField, Min(0f)] private float stopDistance = 0.8f;
        [SerializeField, Min(0f)] private float chaseSpeed = 4f;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private bool spriteFacesRight = true;

        private Rigidbody2D body;
        private EnemyTrainingDummy trainingDummy;
        private PlayerHitDeath targetDeath;
        private EnemyKnockback knockback;

        public Transform Target => player;
        public bool HasLivingTarget => player != null && player.gameObject.activeInHierarchy &&
            (targetDeath == null || !targetDeath.IsDead);

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            knockback = GetComponent<EnemyKnockback>();
            trainingDummy = GetComponent<EnemyTrainingDummy>();
            if (player != null)
                targetDeath = player.GetComponent<PlayerHitDeath>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void FixedUpdate()
        {
            if (player == null)
            {
                var target = GameObject.FindGameObjectWithTag("Player");
                if (target != null)
                {
                    player = target.transform;
                    targetDeath = player.GetComponent<PlayerHitDeath>();
                }
            }
            // Nhường vận tốc cho knockback để AI không kéo quái ngược lại ngay khi bị đẩy.
            if (knockback != null && knockback.IsActive) return;

            float horizontalSpeed = 0f;
            if (HasLivingTarget &&
                (trainingDummy == null || !trainingDummy.IsDefeated))
            {
                Vector2 toPlayer = (Vector2)player.position - body.position;
                if (toPlayer.sqrMagnitude <= detectionRange * detectionRange)
                {
                    FaceDirection(toPlayer.x);
                    if (Mathf.Abs(toPlayer.x) > stopDistance)
                    {
                        float speedToStop = (Mathf.Abs(toPlayer.x) - stopDistance) / Time.fixedDeltaTime;
                        horizontalSpeed = Mathf.Sign(toPlayer.x) * Mathf.Min(chaseSpeed, speedToStop);
                    }
                }
            }

            SetHorizontalSpeed(horizontalSpeed);
        }

        private void OnDisable()
        {
            if (body != null)
                SetHorizontalSpeed(0f);
        }

        public void FaceDirection(float horizontalDirection)
        {
            if (spriteRenderer != null)
                spriteRenderer.flipX = (horizontalDirection > 0f) != spriteFacesRight;
        }

        private void SetHorizontalSpeed(float speed)
        {
            Vector2 velocity = body.linearVelocity;
            velocity.x = speed;
            body.linearVelocity = velocity;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}
