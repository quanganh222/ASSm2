using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
#endif

namespace Metroidvania.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyBatFlight : MonoBehaviour
    {
        [SerializeField] private Vector2 areaOffset;
        [SerializeField] private Vector2 areaSize = new Vector2(6f, 3f);
        [SerializeField, Min(0.1f)] private float flySpeed = 2f;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private bool spriteFacesRight = true;
        [SerializeField] private PlayerHealth player;
        [SerializeField] private Animator animator;
        [SerializeField, Min(0f)] private float idleDuration = 0.8f;
        [SerializeField, Min(0f)] private float detectionRange = 5f;
        [SerializeField, Min(0.01f)] private float attackRange = 0.9f;
        [SerializeField, Min(0.01f)] private float biteRadius = 1.1f;
        [SerializeField, Min(0f)] private float attackCooldown = 1f;
        [SerializeField, Min(1)] private int biteDamage = 10;

        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        private Rigidbody2D body;
        private EnemyKnockback knockback;
        private EnemyTrainingDummy health;
        private Vector2 spawnPosition;
        private Vector2 destination;
        private float idleUntil;
        private float nextAttackTime;
        private bool attacking;
        private bool biteApplied;
        private Collider2D[] playerColliders;

        internal Vector2 AreaCenter => Application.isPlaying
            ? spawnPosition + areaOffset
            : (Vector2)transform.position + areaOffset;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            knockback = GetComponent<EnemyKnockback>();
            health = GetComponent<EnemyTrainingDummy>();
            if (player != null)
                playerColliders = player.GetComponentsInChildren<Collider2D>(true);
            if (animator == null)
                animator = GetComponent<Animator>();
            spawnPosition = body.position;
            body.gravityScale = 0f;
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            PickDestination();
        }

        private void FixedUpdate()
        {
            if (player == null)
            {
                var target = GameObject.FindGameObjectWithTag("Player");
                if (target != null)
                {
                    player = target.GetComponent<PlayerHealth>();
                    playerColliders = target.GetComponentsInChildren<Collider2D>(true);
                }
            }
            if (knockback != null && knockback.IsActive) return;
            
            if (health != null && health.IsDefeated)
            {
                body.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 minimum = AreaCenter - areaSize * 0.5f;
            Vector2 maximum = AreaCenter + areaSize * 0.5f;
            if (attacking)
            {
                StopFlying();
                return;
            }

            bool playerAlive = player != null && player.isActiveAndEnabled && player.CurrentHealth > 0;
            Vector2 targetPosition = playerAlive ? (Vector2)player.transform.position : Vector2.zero;
            bool pursuing = playerAlive &&
                targetPosition.x >= minimum.x && targetPosition.x <= maximum.x &&
                targetPosition.y >= minimum.y && targetPosition.y <= maximum.y &&
                (targetPosition - body.position).sqrMagnitude <= detectionRange * detectionRange;

            if (pursuing)
            {
                destination = targetPosition;
                FaceDirection(targetPosition.x - body.position.x);
                float startBiteDistance = Mathf.Min(attackRange, biteRadius * 0.9f);
                if (DistanceToPlayerSquared() <= startBiteDistance * startBiteDistance)
                {
                    StopFlying();
                    if (Time.time >= nextAttackTime && animator != null)
                    {
                        attacking = true;
                        biteApplied = false;
                        animator.SetTrigger(AttackHash);
                    }
                    return;
                }
            }
            else if (Time.time < idleUntil)
            {
                StopFlying();
                return;
            }

            destination = new Vector2(
                Mathf.Clamp(destination.x, minimum.x, maximum.x),
                Mathf.Clamp(destination.y, minimum.y, maximum.y));
            Vector2 delta = destination - body.position;
            if (delta.sqrMagnitude < 0.01f)
            {
                PickDestination();
                idleUntil = Time.time + idleDuration;
                StopFlying();
                return;
            }

            float speed = Mathf.Min(flySpeed, delta.magnitude / Time.fixedDeltaTime);
            body.linearVelocity = delta.normalized * speed;
            if (animator != null)
                animator.SetBool(IsMovingHash, true);
            FaceDirection(delta.x);
        }

        public void BatBite()
        {
            if (!attacking || biteApplied || (health != null && health.IsDefeated) ||
                player == null || player.CurrentHealth <= 0) return;
            biteApplied = true;
            if (DistanceToPlayerSquared() <= biteRadius * biteRadius)
                player.TakeDamage(biteDamage);
        }

        private float DistanceToPlayerSquared()
        {
            float closest = float.PositiveInfinity;
            if (playerColliders == null) return closest;

            foreach (Collider2D collider in playerColliders)
            {
                if (collider == null || !collider.isActiveAndEnabled || collider.isTrigger)
                    continue;

                float distance = (collider.ClosestPoint(body.position) - body.position).sqrMagnitude;
                closest = Mathf.Min(closest, distance);
            }
            return closest;
        }

        public void FinishBatAttack()
        {
            if (!attacking) return;
            attacking = false;
            nextAttackTime = Time.time + attackCooldown;
        }

        private void StopFlying()
        {
            body.linearVelocity = Vector2.zero;
            if (animator != null)
                animator.SetBool(IsMovingHash, false);
        }

        private void FaceDirection(float deltaX)
        {
            if (spriteRenderer != null && Mathf.Abs(deltaX) > 0.01f)
                spriteRenderer.flipX = (deltaX > 0f) != spriteFacesRight;
        }

        private void PickDestination()
        {
            Vector2 half = areaSize * 0.5f;
            destination = AreaCenter + new Vector2(
                Random.Range(-half.x, half.x),
                Random.Range(-half.y, half.y));
        }

        private void OnValidate()
        {
            areaSize.x = Mathf.Max(0.1f, areaSize.x);
            areaSize.y = Mathf.Max(0.1f, areaSize.y);
        }

        private void OnDisable()
        {
            if (body != null)
                body.linearVelocity = Vector2.zero;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(AreaCenter, areaSize);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, biteRadius);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(EnemyBatFlight))]
    public sealed class EnemyBatFlightEditor : Editor
    {
        private readonly BoxBoundsHandle boundsHandle = new BoxBoundsHandle
        {
            axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y,
            handleColor = Color.cyan,
            wireframeColor = Color.cyan
        };

        private void OnSceneGUI()
        {
            if (EditorApplication.isPlaying) return;
            serializedObject.Update();
            var flight = (EnemyBatFlight)target;
            var offset = serializedObject.FindProperty("areaOffset");
            var size = serializedObject.FindProperty("areaSize");
            Vector3 center = new Vector3(flight.AreaCenter.x, flight.AreaCenter.y, flight.transform.position.z);
            boundsHandle.center = center;
            boundsHandle.size = size.vector2Value;

            EditorGUI.BeginChangeCheck();
            boundsHandle.DrawHandle();
            if (!EditorGUI.EndChangeCheck()) return;

            offset.vector2Value += (Vector2)(boundsHandle.center - center);
            size.vector2Value = new Vector2(
                Mathf.Max(0.1f, boundsHandle.size.x),
                Mathf.Max(0.1f, boundsHandle.size.y));
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
