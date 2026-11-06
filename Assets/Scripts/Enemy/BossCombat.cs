using UnityEngine;

namespace Metroidvania.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(EnemyTrainingDummy))]
    public sealed class BossCombat : MonoBehaviour
    {
        [SerializeField] private PlayerHealth player;
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private bool spriteFacesRight = true;
        [SerializeField, Min(0f)] private float detectionRange = 14f;
        [SerializeField, Min(0f)] private float laserRange = 9f;
        [SerializeField, Min(0f)] private float slashRange = 4f;
        [SerializeField, Min(0f)] private float moveSpeed = 3f;
        [SerializeField, Min(0f)] private float recovery = 1f;
        [SerializeField] private Vector2 slashOffset = new Vector2(2.5f, 0f);
        [SerializeField] private Vector2 slashSize = new Vector2(5f, 4f);
        [SerializeField, Min(1)] private int slashDamage = 25;
        [SerializeField] private Vector2 laserOffset = new Vector2(5f, 0f);
        [SerializeField] private Vector2 laserSize = new Vector2(10f, 1.5f);
        [SerializeField, Min(1)] private int laserDamage = 35;

        private static readonly int Idle = Animator.StringToHash("Base Layer.Idle");
        private static readonly int Run = Animator.StringToHash("Base Layer.Run");
        private static readonly int Slash = Animator.StringToHash("Base Layer.Slash");
        private static readonly int Laser = Animator.StringToHash("Base Layer.Laser");

        private Animator animator;
        private Rigidbody2D body;
        private EnemyKnockback knockback;
        private EnemyTrainingDummy health;
        private int playingState;
        private int attackState;
        private float nextAttackTime;
        private float direction = 1f;
        private bool hitApplied;

        private bool HasLivingPlayer => player != null && player.isActiveAndEnabled && player.CurrentHealth > 0;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            body = GetComponent<Rigidbody2D>();
            knockback = GetComponent<EnemyKnockback>();
            health = GetComponent<EnemyTrainingDummy>();
            if (sprite == null)
                sprite = GetComponentInChildren<SpriteRenderer>();
        }

        private void FixedUpdate()
        {
            if (knockback != null && knockback.IsActive) return;
            
            SetHorizontalSpeed(0f);
            if (health.IsDefeated) return;

            if (attackState != 0)
            {
                var state = animator.GetCurrentAnimatorStateInfo(0);
                if (state.fullPathHash != attackState || state.normalizedTime < 1f) return;

                attackState = 0;
                nextAttackTime = Time.time + recovery;
            }

            if (!HasLivingPlayer)
            {
                Play(Idle);
                return;
            }

            Vector2 delta = (Vector2)player.transform.position - body.position;
            float distance = delta.magnitude;
            if (distance > detectionRange)
            {
                Play(Idle);
                return;
            }

            if (Mathf.Abs(delta.x) > 0.01f)
                direction = Mathf.Sign(delta.x);
            if (sprite != null)
                sprite.flipX = (direction > 0f) != spriteFacesRight;

            if (distance > laserRange)
            {
                SetHorizontalSpeed(direction * moveSpeed);
                Play(Run);
            }
            else if (Time.time >= nextAttackTime)
            {
                attackState = distance <= slashRange ? Slash : Laser;
                hitApplied = false;
                Play(attackState);
            }
            else
            {
                Play(Idle);
            }
        }

        public void SlashHit() => ApplyHit(Slash, slashOffset, slashSize, slashDamage);
        public void LaserHit() => ApplyHit(Laser, laserOffset, laserSize, laserDamage);

        private void ApplyHit(int expectedAttack, Vector2 offset, Vector2 size, int damage)
        {
            if (attackState != expectedAttack || hitApplied || health.IsDefeated || !HasLivingPlayer)
                return;

            hitApplied = true;
            foreach (Collider2D collider in Physics2D.OverlapBoxAll(HitCenter(offset), size, 0f))
            {
                if (collider.isTrigger || collider.GetComponentInParent<PlayerHealth>() != player)
                    continue;

                player.TakeDamage(damage);
                break;
            }
        }

        private void Play(int state)
        {
            if (playingState == state) return;
            animator.Play(state, 0, 0f);
            playingState = state;
        }

        private void SetHorizontalSpeed(float speed)
        {
            Vector2 velocity = body.linearVelocity;
            velocity.x = speed;
            body.linearVelocity = velocity;
        }

        private Vector3 HitCenter(Vector2 offset) =>
            transform.position + new Vector3(offset.x * direction, offset.y, 0f);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(HitCenter(slashOffset), slashSize);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(HitCenter(laserOffset), laserSize);
        }
    }
}
