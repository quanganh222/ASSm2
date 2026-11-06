using UnityEngine;

namespace Metroidvania.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class PlayerHealthBarUI : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private RectTransform healthRegion;
        [SerializeField] private RectTransform missingHealth;

        private void Awake()
        {
            if (playerHealth == null || healthRegion == null || missingHealth == null ||
                missingHealth.parent != healthRegion)
            {
                Debug.LogError("PlayerHealthBarUI: Assign Player Health, Health Region and its child Missing Health.", this);
                enabled = false;
                return;
            }

            missingHealth.anchorMin = new Vector2(1f, 0f);
            missingHealth.anchorMax = new Vector2(1f, 1f);
            missingHealth.pivot = new Vector2(1f, 0.5f);
            missingHealth.anchoredPosition = Vector2.zero;
            missingHealth.sizeDelta = Vector2.zero;
        }

        private void LateUpdate()
        {
            float healthFraction = playerHealth.MaxHealth > 0
                ? Mathf.Clamp01((float)playerHealth.CurrentHealth / playerHealth.MaxHealth)
                : 0f;

            missingHealth.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                healthRegion.rect.width * (1f - healthFraction));
        }
    }
}
