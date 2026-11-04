using UnityEngine;

namespace Metroidvania.Gameplay
{
    [RequireComponent(typeof(EnemyChase), typeof(Animator))]
    [DisallowMultipleComponent]
    public sealed class EnemyWebShooter : MonoBehaviour
    {
        [SerializeField] private EnemyWebProjectile webPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField, Min(0.1f)] private float attackRange = 4f;
        [SerializeField, Min(0.1f)] private float attackCooldown = 1.5f;

        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private Animator animator;
        private EnemyChase chase;
        private EnemyTrainingDummy health;
        private float nextAttackTime;
        private bool waitingToFire;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            chase = GetComponent<EnemyChase>();
            health = GetComponent<EnemyTrainingDummy>();
        }

        private void Update()
        {
            Transform target = chase.Target;
            if (webPrefab == null || !chase.HasLivingTarget ||
                (health != null && health.IsDefeated) || Time.time < nextAttackTime)
                return;

            if (animator.IsInTransition(0) ||
                animator.GetCurrentAnimatorStateInfo(0).shortNameHash != Animator.StringToHash("Run"))
                return;

            Vector2 delta = target.position - transform.position;
            if (delta.sqrMagnitude > attackRange * attackRange) return;

            nextAttackTime = Time.time + attackCooldown;
            waitingToFire = true;
            animator.SetTrigger(AttackHash);
        }

        // Animation Event on the frame where the spider releases the web.
        public void ShootWeb()
        {
            Transform target = chase.Target;
            if (webPrefab == null || target == null)
            {
                Debug.LogWarning("EnemyWebShooter: Assign Web Prefab and Enemy Chase > Player.", this);
                return;
            }

            if (!chase.HasLivingTarget ||
                (health != null && health.IsDefeated)) return;

            Vector3 spawn = firePoint != null ? firePoint.position : transform.position;
            // EnemyChase flips only the sprite. Mirror the mouth offset separately.
            float side = target.position.x >= transform.position.x ? 1f : -1f;
            spawn.x = transform.position.x + Mathf.Abs(spawn.x - transform.position.x) * side;
            Vector2 direction = (Vector2)(target.position - spawn);
            if (direction.sqrMagnitude < 0.0001f) direction = Vector2.right * side;

            var web = Instantiate(webPrefab, spawn, Quaternion.identity);
            web.Launch(direction, target);
        }
    }
}
