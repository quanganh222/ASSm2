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

        private PlayerHitDeath reactions;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;

        private void Awake()
        {
            reactions = GetComponent<PlayerHitDeath>();
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || CurrentHealth <= 0 || reactions.IsDead)
                return;

            currentHealth = Mathf.Max(0, CurrentHealth - damage);
            if (CurrentHealth == 0)
                reactions.Die();
            else
                reactions.PlayHit();
        }
    }
}
