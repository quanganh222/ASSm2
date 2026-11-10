using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Metroidvania.Gameplay
{
    // Giữ yêu cầu nhảy từ Update đến bước vật lý kế tiếp để tránh bỏ sót lần bấm Space.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMotor2D))]
    public sealed class PlayerJump : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float jumpSpeed = 10f;
        [SerializeField] private LayerMask groundLayer;

        private PlayerMotor2D motor;
        private Rigidbody2D body;
        private ContactFilter2D groundContactFilter;
        private bool jumpRequested;
        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            motor = GetComponent<PlayerMotor2D>();
            body = GetComponent<Rigidbody2D>();
            if (groundLayer.value == 0)
            {
                Debug.LogError("PlayerJump: Assign Ground Layer.", this);
                enabled = false;
                return;
            }

            groundContactFilter.SetLayerMask(groundLayer);
            // Chỉ nhận tiếp xúc hướng lên từ lớp Ground để bám sàn và dốc khi xét chạm đất.
            groundContactFilter.SetNormalAngle(30f, 150f);
        }

        private void Update()
        {
            jumpRequested |= ReadJumpPressed();
        }

        private void FixedUpdate()
        {
            // Dùng tiếp xúc thật của Rigidbody2D và loại lúc Player đang đi lên để tránh nhảy liên tiếp trên không.
            IsGrounded = body.linearVelocity.y <= 0.01f &&
                body.IsTouching(groundContactFilter);

            if (jumpRequested && IsGrounded && !motor.IsDashing)
            {
                motor.Jump(jumpSpeed);
                IsGrounded = false;
            }
            jumpRequested = false;
        }

        private void OnDisable()
        {
            jumpRequested = false;
            IsGrounded = false;
        }

        private static bool ReadJumpPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Space);
#else
            return false;
#endif
        }
    }
}
