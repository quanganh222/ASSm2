using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Metroidvania.Gameplay
{
    // Hiện số sát thương tại điểm trúng đòn. Chữ nổi lên, mờ dần và tự dọn khi đổi scene.
    [DefaultExecutionOrder(1000)]
    public sealed class DamageNumber : MonoBehaviour
    {
        private const float Lifetime = 0.9f;
        private const float Rise = 70f;

        private Text label;
        private RectTransform canvasRect;
        private Vector3 worldPosition;
        private Scene scene;
        private Color color;
        private float elapsed;

        internal void Show(Text text, RectTransform canvas, Vector3 position)
        {
            label = text;
            canvasRect = canvas;
            worldPosition = position;
            scene = SceneManager.GetActiveScene();
            color = label.color;
            UpdatePosition(0f);
        }

        private void LateUpdate()
        {
            elapsed += Time.unscaledDeltaTime;
            if (elapsed >= Lifetime || SceneManager.GetActiveScene() != scene)
            {
                Destroy(gameObject);
                return;
            }

            float progress = elapsed / Lifetime;
            color.a = 1f - Mathf.InverseLerp(0.45f, 1f, progress);
            label.color = color;
            UpdatePosition(progress);
        }

        private void UpdatePosition(float progress)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                label.enabled = false;
                return;
            }

            // Đổi tọa độ thế giới sang tọa độ Canvas mỗi frame để chữ bám đúng điểm khi camera di chuyển.
            Vector3 screen = camera.WorldToScreenPoint(worldPosition);
            label.enabled = screen.z > 0f;
            if (label.enabled && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screen, null, out var position))
            {
                label.rectTransform.anchoredPosition = position + Vector2.up * (progress * Rise);
            }
        }
    }
}
