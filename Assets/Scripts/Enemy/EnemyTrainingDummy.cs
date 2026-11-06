using System;
using UnityEngine;

namespace Metroidvania.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class EnemyTrainingDummy : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField, Min(1)] private int hitsToDefeat = 3;
        [SerializeField] private bool despawnOnDefeat;
        [SerializeField, Min(0f)] private float despawnDelay = 3f;
        [SerializeField, Min(0.01f)] private float hitFlashDuration = 0.15f;

        private int hitsTaken;
        public int MaxHealth => hitsToDefeat;
        public int CurrentHealth => Mathf.Max(0, hitsToDefeat - hitsTaken);
        public bool IsDefeated => hitsTaken >= hitsToDefeat;
        public event Action HitReceived;
        private Color originalColor;
        private float flashUntil;
        private bool isFlashing;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
        }

        public void ReceiveHit() => ReceiveHitFrom(transform.position + Vector3.left);

        public void ReceiveHitFrom(Vector2 attackerPosition)
        {
            if (hitsTaken >= hitsToDefeat) return;

            hitsTaken++;
            HitReceived?.Invoke();
            bool defeated = hitsTaken >= hitsToDefeat;
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