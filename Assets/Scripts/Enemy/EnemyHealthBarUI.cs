using UnityEngine;
using UnityEngine.UI;

namespace Metroidvania.Gameplay
{
    // Hiện máu trên đầu quái lúc còn sống. Độ đầy của Slider lấy từ lượng máu thực tế.
    [DisallowMultipleComponent]
    public sealed class EnemyHealthBarUI : MonoBehaviour
    {
        [SerializeField] private EnemyTrainingDummy enemy;
        [SerializeField] private Slider healthSlider;
        [SerializeField, Min(0f)] private float heightAboveEnemy = 0.25f;

        private SpriteRenderer enemySprite;
        private Canvas worldCanvas;
        private RectTransform sliderRect;
        private RectTransform backgroundRect;
        private RectTransform fillAreaRect;

        private void Awake()
        {
            if (enemy == null || healthSlider == null)
            {
                Debug.LogError("EnemyHealthBarUI: Assign Enemy and Health Slider.", this);
                enabled = false;
                return;
            }

            enemySprite = enemy.GetComponentInChildren<SpriteRenderer>();
            worldCanvas = healthSlider.GetComponentInParent<Canvas>();
            sliderRect = healthSlider.transform as RectTransform;
            backgroundRect = healthSlider.transform.Find("Background") as RectTransform;
            if (healthSlider.fillRect != null)
                fillAreaRect = healthSlider.fillRect.parent as RectTransform;
            healthSlider.interactable = false;
            healthSlider.wholeNumbers = false;
            healthSlider.direction = Slider.Direction.LeftToRight;
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            Refresh();
            PositionAboveEnemy();
        }

        private void LateUpdate()
        {
            if (enemy != null && healthSlider != null)
            {
                Refresh();
                PositionAboveEnemy();
            }
        }

        private void PositionAboveEnemy()
        {
            if (enemySprite == null || worldCanvas == null || sliderRect == null ||
                worldCanvas.renderMode != RenderMode.WorldSpace) return;

            // flipX chỉ lật hình. Dùng bounds của sprite để căn thanh máu theo hình đang hiển thị.
            Bounds bounds = enemySprite.bounds;
            Vector3 target = new Vector3(bounds.center.x,
                bounds.max.y + heightAboveEnemy, enemySprite.transform.position.z);
            Vector3 current = sliderRect.TransformPoint(sliderRect.rect.center);
            worldCanvas.transform.position += target - current;
            worldCanvas.overrideSorting = true;
            worldCanvas.sortingLayerID = enemySprite.sortingLayerID;
            worldCanvas.sortingOrder = enemySprite.sortingOrder + 10;
        }

        private void Refresh()
        {
            // Căn nền và vùng tô trên cùng một trục, chừa viền trái và phải bằng nhau.
            if (backgroundRect != null)
                SetTrackRect(backgroundRect, Vector2.zero);
            if (fillAreaRect != null && fillAreaRect != sliderRect)
                SetTrackRect(fillAreaRect, new Vector2(2f, 1f));

            float ratio = enemy.MaxHealth > 0
                ? Mathf.Clamp01((float)enemy.CurrentHealth / enemy.MaxHealth)
                : 0f;
            healthSlider.gameObject.SetActive(!enemy.IsDefeated);
            healthSlider.SetValueWithoutNotify(ratio);

            // Đặt offset của Fill về 0 để phần đỏ chỉ rộng theo tỷ lệ máu, kể cả frame đầu.
            RectTransform fill = healthSlider.fillRect;
            if (fill != null)
            {
                fill.anchorMin = Vector2.zero;
                fill.anchorMax = new Vector2(ratio, 1f);
                fill.offsetMin = Vector2.zero;
                fill.offsetMax = Vector2.zero;
                Image fillImage = fill.GetComponent<Image>();
                if (fillImage != null)
                    fillImage.type = fillImage.sprite != null && fillImage.sprite.border != Vector4.zero
                        ? Image.Type.Sliced : Image.Type.Simple;
            }
        }

        private static void SetTrackRect(RectTransform rect, Vector2 padding)
        {
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.anchorMin = new Vector2(0f, 0.25f);
            rect.anchorMax = new Vector2(1f, 0.75f);
            rect.offsetMin = padding;
            rect.offsetMax = -padding;
        }
    }
}
