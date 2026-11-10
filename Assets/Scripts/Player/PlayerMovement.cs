using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Metroidvania.Gameplay
{
    // Đọc trái/phải trong Update rồi gửi vận tốc ngang cho motor trong FixedUpdate.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMotor2D))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 5f;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private bool spriteFacesRight = true;

        private PlayerMotor2D motor;
        private float horizontalInput;
        public float HorizontalInput => horizontalInput;
        public bool FacingRight { get; private set; } = true;

        private void Awake()
        {
            motor = GetComponent<PlayerMotor2D>();
            FacingRight = spriteFacesRight;
        }

        private void Update()
        {
            horizontalInput = ReadHorizontal();
            if (horizontalInput == 0f || motor.IsDashing) return;

            // Chỉ lật sprite để giữ collider ổn định. Hướng nhìn được dùng chung cho dash và hitbox.
            FacingRight = horizontalInput > 0f;
            if (spriteRenderer != null)
                spriteRenderer.flipX = FacingRight != spriteFacesRight;
        }

        private void FixedUpdate() => motor.MoveHorizontal(horizontalInput * moveSpeed);

        private void OnDisable()
        {
            horizontalInput = 0f;
            if (motor != null) motor.MoveHorizontal(0f);
        }

        private static float ReadHorizontal()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null) return 0f;
            bool left = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
            bool right = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
            return (right ? 1f : 0f) - (left ? 1f : 0f);
#elif ENABLE_LEGACY_INPUT_MANAGER
            bool left = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
            bool right = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
            return (right ? 1f : 0f) - (left ? 1f : 0f);
#else
            return 0f;
#endif
        }
    }
}
