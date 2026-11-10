using UnityEngine;

namespace Metroidvania.Gameplay
{
    // Sâu áp sát rồi chờ hết thời gian Fuse mới nổ. Một nhát chém có thể kích nổ sớm.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyChase), typeof(Animator))]
    public sealed class EnemyExplosion : MonoBehaviour
    {
        private static readonly int ExplodeHash = Animator.StringToHash("Explode");

        [SerializeField, Min(0.01f)] private float triggerRadius = 1.4f;
        [SerializeField, Min(0.01f)] private float blastRadius = 1.8f;
        [SerializeField, Min(0f)] private float fuseSeconds = 1f;
        [SerializeField, Min(1)] private int blastDamage = 25;

        private Animator animator;
        private EnemyChase chase;
        private EnemyTrainingDummy trainingDummy;
        private Collider2D[] targetColliders;
        private float detonateAt;
        private bool armed;
        private bool detonating;
        private bool blastApplied;
        private bool detonatedByHit;
        public bool IsArmed => armed;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            chase = GetComponent<EnemyChase>();
            trainingDummy = GetComponent<EnemyTrainingDummy>();
            if (chase.Target != null)
                targetColliders = chase.Target.GetComponentsInChildren<Collider2D>(true);
        }

        private void OnEnable()
        {
            if (trainingDummy != null)
                trainingDummy.HitReceived += DetonateFromHit;
        }

        private void OnDisable()
        {
            if (trainingDummy != null)
                trainingDummy.HitReceived -= DetonateFromHit;
        }

        private void FixedUpdate()
        {
            if (detonating || !chase.HasLivingTarget) return;
            if (trainingDummy != null && trainingDummy.IsDefeated)
            {
                enabled = false;
                return;
            }

            if (!armed && chase.isActiveAndEnabled && IsTargetWithin(triggerRadius))
                Arm();

            if (!armed || Time.fixedTime < detonateAt) return;

            Detonate();
        }

        private void Detonate()
        {
            if (detonating || !chase.HasLivingTarget) return;

            if (!armed)
                Arm();

            detonating = true;
            animator.SetTrigger(ExplodeHash);
        }

        private void DetonateFromHit()
        {
            if (detonating || !chase.HasLivingTarget) return;

            detonatedByHit = true;
            Detonate();
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!isActiveAndEnabled || armed || !chase.isActiveAndEnabled || !chase.HasLivingTarget ||
                (trainingDummy != null && trainingDummy.IsDefeated)) return;

            Transform target = chase.Target;
            Transform other = collision.collider.transform;
            if (target != null && (other == target || other.IsChildOf(target)))
                Arm();
        }

        private void Arm()
        {
            armed = true;
            detonateAt = Time.fixedTime + fuseSeconds;
            chase.enabled = false;
        }

        // Animation Event ở frame đầu thấy vụ nổ. Kiểm tra bán kính ngay tại thời điểm này.
        public void ApplyBlast()
        {
            if (!detonating || blastApplied) return;
            blastApplied = true;

            // Nếu nhát chém đã kết liễu sâu trước frame nổ, chỉ giữ hiệu ứng và bỏ sát thương.
            if (trainingDummy != null && trainingDummy.IsDefeated) return;
            if (!IsTargetWithin(blastRadius)) return;

            if (chase.Target.TryGetComponent<PlayerHealth>(out var playerHealth))
                playerHealth.TakeDamage(blastDamage);
            else if (chase.Target.TryGetComponent<PlayerHitDeath>(out var playerHitDeath))
                playerHitDeath.PlayHit();
        }

        // Animation Event ở frame cuối thấy vụ nổ để dọn con sâu.
        public void FinishExplosion()
        {
            if (!detonating) return;

            detonating = false;
            // Ghi điểm cho vụ nổ do đòn đánh của Player hoặc khi sâu đã bị kết liễu.
            if (detonatedByHit || (trainingDummy != null && trainingDummy.IsDefeated))
                GameManager.Instance?.AwardKill();
            Destroy(gameObject);
        }

        private bool IsTargetWithin(float radius)
        {
            Transform target = chase.Target;
            if (!chase.HasLivingTarget)
                return false;

            Vector2 center = transform.position;
            float radiusSquared = radius * radius;
            if (targetColliders != null)
            {
                foreach (Collider2D targetCollider in targetColliders)
                {
                    if (targetCollider == null || !targetCollider.isActiveAndEnabled ||
                        targetCollider.isTrigger) continue;

                    if ((targetCollider.ClosestPoint(center) - center).sqrMagnitude <= radiusSquared)
                        return true;
                }
                return false;
            }

            return ((Vector2)target.position - center).sqrMagnitude <= radiusSquared;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, blastRadius);
        }
    }
}
