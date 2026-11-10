using UnityEngine;

namespace Metroidvania.Gameplay
{
    // Tập trung các thao tác ghi vận tốc vào một component. Dash được ưu tiên cho tới khi trả lại quyền di chuyển và nhảy.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMotor2D : MonoBehaviour
    {
        private Rigidbody2D body;
        private float gravityBeforeDash;

        public bool IsDashing { get; private set; }

        private void Awake() => body = GetComponent<Rigidbody2D>();

        // Chỉ đổi vận tốc ngang để chạy không cắt mất đà nhảy hoặc rơi.
        public void MoveHorizontal(float speed)
        {
            if (IsDashing) return;

            Vector2 velocity = body.linearVelocity;
            velocity.x = speed;
            body.linearVelocity = velocity;
        }

        // Chỉ đổi vận tốc dọc để Player vẫn di chuyển ngang khi nhảy.
        public void Jump(float speed)
        {
            if (IsDashing) return;
            Vector2 velocity = body.linearVelocity;
            velocity.y = speed;
            body.linearVelocity = velocity;
        }

        public void BeginDash(float horizontalSpeed)
        {
            if (IsDashing) return;

            // Lưu trọng lực đang dùng trước khi tắt nó để lướt ngang rồi trả lại đúng giá trị khi kết thúc.
            gravityBeforeDash = body.gravityScale;
            IsDashing = true;
            body.gravityScale = 0f;
            body.linearVelocity = new Vector2(horizontalSpeed, 0f);
        }

        public void EndDash()
        {
            if (!IsDashing) return;

            IsDashing = false;
            body.gravityScale = gravityBeforeDash;
            MoveHorizontal(0f);
        }
    }
}
