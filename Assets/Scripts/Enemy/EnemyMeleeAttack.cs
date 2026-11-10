using UnityEngine;

namespace Metroidvania.Gameplay
{
    // Điều phối đòn chém gần: ngừng đuổi, quay về phía Player, phát clip rồi trả lại quyền đuổi.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyChase), typeof(Animator), typeof(EnemyTrainingDummy))]
    public sealed class EnemyMeleeAttack : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float attackRange = 1.4f;
        [SerializeField, Min(0f)] private float cooldown = 0.8f;
        [SerializeField] private Vector2 hitOffset = new Vector2(0.85f, 0f);
        [SerializeField] private Vector2 hitSize = new Vector2(1.3f, 1.5f);
        [SerializeField, Min(1)] private int damage = 15;

        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int Moving = Animator.StringToHash("IsMoving");
        private Animator animator;
        private EnemyChase chase;
        private EnemyTrainingDummy health;
        private Rigidbody2D body;
        private EnemyKnockback knockback;
        private float nextAttackTime;
        private float direction = 1f;
        private bool attacking;
        private bool hitChecked;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            chase = GetComponent<EnemyChase>();
            health = GetComponent<EnemyTrainingDummy>();
            body = GetComponent<Rigidbody2D>();
            knockback = GetComponent<EnemyKnockback>();
        }

        private void Update()
        {
            if (health.IsDefeated || (knockback != null && knockback.IsActive)) return;

            animator.SetBool(Moving, Mathf.Abs(body.linearVelocity.x) > 0.05f);
            if (attacking || !chase.isActiveAndEnabled || !chase.HasLivingTarget ||
                Time.time < nextAttackTime) return;

            Vector2 delta = chase.Target.position - transform.position;
            if (delta.sqrMagnitude > attackRange * attackRange) return;

            direction = delta.x < 0f ? -1f : 1f;
            chase.FaceDirection(direction);
            attacking = true;
            hitChecked = false;
            chase.enabled = false;
            animator.SetTrigger(Attack);
        }

        // Chỉ kiểm tra vùng chém ở frame Animation Event để Player có thể né dù đã vào tầm ra đòn.
        public void MeleeHit()
        {
            if (!attacking || hitChecked || health.IsDefeated || !chase.HasLivingTarget) return;
            hitChecked = true;
            if (!chase.Target.TryGetComponent<PlayerHealth>(out var playerHealth)) return;

            foreach (Collider2D collider in Physics2D.OverlapBoxAll(HitCenter, hitSize, 0f))
            {
                if (collider.isTrigger || collider.GetComponentInParent<PlayerHealth>() != playerHealth)
                    continue;

                playerHealth.TakeDamage(damage);
                break;
            }
        }

        public void FinishMeleeAttack()
        {
            if (!attacking) return;
            attacking = false;
            nextAttackTime = Time.time + cooldown;
            if (!health.IsDefeated)
                chase.enabled = true;
        }

        private Vector3 HitCenter =>
            transform.position + new Vector3(hitOffset.x * direction, hitOffset.y, 0f);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(HitCenter, hitSize);
        }
    }
}
