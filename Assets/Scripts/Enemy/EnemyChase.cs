using UnityEngine;

namespace Metroidvania.Gameplay
{
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

        public Transform Target => player;
        public bool HasLivingTarget => player != null && player .gameObject.activeInHierarchy &&
            (targetDeath == null || !targetDeath.IsDead);

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            trainingDummy = GetComponent<EnemyTrainingDummy>();
             if (player != null)
                targetDeath = player.GetComponent<PlayerHitDeath>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void FixedUpdate()
        {
            float horizontalSpeed = 0f;
             if (HasLivingTarget &&
                (trainingDummy == null || !trainingDummy.IsDefeated))
            {
                Vector2 toPlayer = (Vector2)player.position - body.position;
                if (toPlayer.sqrMagnitude <= detectionRange * detectionRange &&
                    Mathf.Abs(toPlayer.x) > stopDistance)
                {
                    float speedToStop = (Mathf.Abs(toPlayer.x) - stopDistance) / Time.fixedDeltaTime;
                    horizontalSpeed = Mathf.Sign(toPlayer.x) * Mathf.Min(chaseSpeed, speedToStop);
                    if (spriteRenderer != null)
                        spriteRenderer.flipX = (toPlayer.x > 0f) != spriteFacesRight;
                }
            }

            SetHorizontalSpeed(horizontalSpeed);
        }

        private void OnDisable()
        {
            if (body != null)
                SetHorizontalSpeed(0f);
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
