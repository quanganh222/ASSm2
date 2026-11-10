using UnityEngine;

namespace Metroidvania.Gameplay
{
    // Đọc năng lượng từ PlayerEnergy và vẽ phần đã mất bằng lớp phủ co giãn từ mép phải.
    [DisallowMultipleComponent]
    public sealed class PlayerEnergyBarUI : MonoBehaviour
    {
        [SerializeField] private PlayerEnergy playerEnergy;
        [SerializeField] private RectTransform energyRegion;
        [SerializeField] private RectTransform missingEnergy;
        internal RectTransform Region => energyRegion;

        private void Awake()
        {
            if (playerEnergy == null || energyRegion == null || missingEnergy == null ||
                missingEnergy.parent != energyRegion)
            {
                Debug.LogError("PlayerEnergyBarUI: Assign Player Energy, Energy Region and its child Missing Energy.", this);
                enabled = false;
                return;
            }

            missingEnergy.anchorMin = new Vector2(1f, 0f);
            missingEnergy.anchorMax = new Vector2(1f, 1f);
            missingEnergy.pivot = new Vector2(1f, 0.5f);
            missingEnergy.anchoredPosition = Vector2.zero;
            missingEnergy.sizeDelta = Vector2.zero;
        }

        private void LateUpdate()
        {
            if (playerEnergy == null || energyRegion == null || missingEnergy == null) return;

            float fraction = playerEnergy.MaxEnergy > 0f
                ? Mathf.Clamp01(playerEnergy.CurrentEnergy / playerEnergy.MaxEnergy)
                : 0f;
            missingEnergy.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal, energyRegion.rect.width * (1f - fraction));
        }
    }
}
