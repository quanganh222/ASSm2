using UnityEngine;

namespace Metroidvania.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerHitDeath))]
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maxHealth = 100;
        [SerializeField, Tooltip("Runtime value. Set Max Health to change starting health.")]
        private int currentHealth;

        [Header("Regeneration")]
        [SerializeField, Min(0f)] private float regenerationDelay = 3f;
        [SerializeField, Min(0f)] private float regenerationPerSecond = 10f;

        [Header("Health Bar")]
        [SerializeField] private RectTransform healthRegion;
        [SerializeField] private RectTransform missingHealth;

        private PlayerHitDeath reactions;
        private bool regenerationArmed;
        private float regenerateAfter;
        private float healingProgress;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        internal RectTransform Region => healthRegion;

        private void Awake()
        {
            reactions = GetComponent<PlayerHitDeath>();
            currentHealth = maxHealth;

            if (healthRegion != null && missingHealth != null && missingHealth.parent == healthRegion)
            {
                missingHealth.anchorMin = new Vector2(1f, 0f);
                missingHealth.anchorMax = new Vector2(1f, 1f);
                missingHealth.pivot = new Vector2(1f, 0.5f);
                missingHealth.anchoredPosition = Vector2.zero;
                missingHealth.sizeDelta = Vector2.zero;
            }
        }

        internal void OnEnemyDefeated()
        {
            if (CurrentHealth <= 0 || reactions.IsDead || GameManager.Instance?.RunEnded == true)
                return;

            regenerationArmed = true;
            RestartRegenerationDelay();
        }

        private void RestartRegenerationDelay()
        {
            regenerateAfter = Time.time + regenerationDelay;
            healingProgress = 0f;
        }

        private void Update()
        {
            if (!regenerationArmed || CurrentHealth <= 0 || reactions.IsDead ||
                GameManager.Instance?.RunEnded == true || Time.time < regenerateAfter)
                return;

            float healingTime = Mathf.Min(Time.deltaTime, Time.time - regenerateAfter);
            healingProgress += healingTime * regenerationPerSecond;
            int restored = Mathf.Min(MaxHealth - CurrentHealth, Mathf.FloorToInt(healingProgress));
            currentHealth += restored;
            healingProgress -= restored;
            if (CurrentHealth == MaxHealth)
                regenerationArmed = false;
        }

        private void LateUpdate()
        {
            if (healthRegion == null || missingHealth == null || missingHealth.parent != healthRegion)
                return;

            float healthFraction = MaxHealth > 0 ? Mathf.Clamp01((float)CurrentHealth / MaxHealth) : 0f;
            missingHealth.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal, healthRegion.rect.width * (1f - healthFraction));
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || CurrentHealth <= 0 || reactions.IsDead || GameManager.Instance?.RunEnded == true)
                return;

            int appliedDamage = Mathf.Min(damage, currentHealth);
            currentHealth -= appliedDamage;
            if (regenerationArmed)
                RestartRegenerationDelay();
            GameManager.Instance?.ShowDamage(transform, appliedDamage, playerTookDamage: true);
            if (CurrentHealth == 0)
                reactions.Die();
            else
                reactions.PlayHit();
        }
    }
}
