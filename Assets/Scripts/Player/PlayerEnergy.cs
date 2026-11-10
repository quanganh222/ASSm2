using UnityEngine;

namespace Metroidvania.Gameplay
{
    // Quản lý năng lượng dùng cho dash. Mỗi lần tiêu hao thành công sẽ lùi thời điểm bắt đầu hồi.
    [DisallowMultipleComponent]
    public sealed class PlayerEnergy : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maxEnergy = 100f;
        [SerializeField, Min(0f)] private float regenerationPerSecond = 15f;
        [SerializeField, Min(0f)] private float regenerationDelay = 1f;
        [SerializeField] private float currentEnergy;

        private PlayerHitDeath death;
        private float regenerateAfter;

        public float MaxEnergy => maxEnergy;
        public float CurrentEnergy => currentEnergy;

        private void Awake()
        {
            currentEnergy = maxEnergy;
            death = GetComponent<PlayerHitDeath>();
        }

        private void Update()
        {
            if ((death != null && death.IsDead) || Time.time < regenerateAfter)
                return;

            currentEnergy = Mathf.Min(maxEnergy, currentEnergy + regenerationPerSecond * Time.deltaTime);
        }

        // Trả về false nếu chi phí không hợp lệ, Player đã chết hoặc năng lượng còn lại không đủ.
        public bool TrySpend(float amount)
        {
            if (amount < 0f || (death != null && death.IsDead) || currentEnergy < amount)
                return false;

            if (amount == 0f) return true;
            currentEnergy -= amount;
            regenerateAfter = Time.time + regenerationDelay;
            return true;
        }
    }
}
