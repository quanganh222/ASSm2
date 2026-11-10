using System;
using UnityEngine;

namespace Metroidvania.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class EnemyTrainingDummy : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField, Min(1)] private int maxHealth = 100;
        [SerializeField] private bool despawnOnDefeat;
        [SerializeField, Min(0f)] private float despawnDelay = 3f;
        [SerializeField, Min(0.01f)] private float hitFlashDuration = 0.15f;

        public int MaxHealth => maxHealth;
        public int CurrentHealth { get; private set; }
        public bool IsDefeated => CurrentHealth <= 0;
        public event Action HitReceived;
        private Color originalColor;
        private float flashUntil;
        private bool isFlashing;
        private AnimationSound animationSound;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            animationSound = AnimationSound.Attach(animator != null ? animator : GetComponentInChildren<Animator>());
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
        }

        public void ReceiveHit() => ReceiveHitFrom(transform.position + Vector3.left);

        public void ReceiveHitFrom(Vector2 attackerPosition, int damege = 25)
        {
            if (damege <= 0 || IsDefeated) return;

            int applinedDamege = Mathf.Min(damege, CurrentHealth);
            CurrentHealth -= applinedDamege;
            GameManager.Instance?.ShowDamage(transform, applinedDamege, playerTookDamage: false);
            HitReceived?.Invoke();
            bool defeated = IsDefeated;
            var explosion = GetComponent<EnemyExplosion>();
            var knockback = GetComponent<EnemyKnockback>();
            if (!defeated && (explosion == null || !explosion.IsArmed) && knockback != null)
            {
                knockback.PushAwayFrom(attackerPosition);
            }
                
            if (defeated && despawnOnDefeat)
                Destroy(gameObject, despawnDelay);
            if (animator != null)
            {
                if (defeated)
                    animator.SetBool("IsDead", true);
                else
                    animator.SetTrigger("Hit");
            }

            if (defeated && explosion == null)
            {
                GameManager.Instance?.AwardKill();
                GameManager.Instance?.BossDefeated(this);
            }
            else if (!defeated)
            {
                animationSound?.PlayAnimationSound(AnimationSound.Impact);
            }

            if (spriteRenderer == null) return;

            flashUntil = Time.time + hitFlashDuration;
            isFlashing = true;
            ApplyFlashColor();
        }

        private void LateUpdate()
        {
            if (!isFlashing) return;

            if (Time.time < flashUntil && spriteRenderer != null)
                ApplyFlashColor();
            else
                StopFlash();
        }

        private void ApplyFlashColor()
        {
            spriteRenderer.color = originalColor * new Color(1f, 0.2f, 0.2f, 1f);
        }

        private void OnDisable()
        {
            StopFlash();
        }

        private void StopFlash()
        {
            if (!isFlashing) return;

            isFlashing = false;
            if (spriteRenderer != null)
                spriteRenderer.color = IsDefeated && animator == null
                    ? originalColor * new Color(0.35f, 0.35f, 0.35f, 1f)
                    : originalColor;
        }

        [ContextMenu("Test Hit")]
        private void TestHit()
        {
            if (Application.isPlaying)
                ReceiveHit();
        }
    }
}
