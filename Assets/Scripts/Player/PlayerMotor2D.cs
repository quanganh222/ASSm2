using UnityEngine;

namespace Metroidvania.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMotor2D : MonoBehaviour
    {
        private Rigidbody2D body;
        private float gravityBeforeDash;

        public bool IsDashing { get; private set; }

        private void Awake() => body = GetComponent<Rigidbody2D>();

        // Chi thay doi truc X, giu nguyen van toc nhay/roi.
        public void MoveHorizontal(float speed)
        {
            if (IsDashing) return;

            Vector2 velocity = body.linearVelocity;
            velocity.x = speed;
            body.linearVelocity = velocity;
        }

        // Chi thay doi truc Y, giu nguyen van toc di ngang.
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
