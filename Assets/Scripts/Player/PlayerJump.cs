using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Metroidvania.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMotor2D))]
    public sealed class PlayerJump : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float jumpSpeed = 10f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.4f, 0.08f);
        [SerializeField] private Vector2 groundCheckOffset;

        private Vector3 GroundCheckCenter => groundCheck.position + (Vector3)groundCheckOffset;

        private PlayerMotor2D motor;
        private Rigidbody2D body;
        private bool jumpRequested;
        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            motor = GetComponent<PlayerMotor2D>();
            body = GetComponent<Rigidbody2D>();
            if (groundCheck == null || groundLayer.value == 0)
            {
                Debug.LogError("PlayerJump: Assign Ground Check and Ground Layer.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            // Giu lenh nhay cho den buoc vat ly tiep theo.
            jumpRequested |= ReadJumpPressed();
        }

        private void FixedUpdate()
        {
            IsGrounded = body.linearVelocity.y <= 0.01f &&
                Physics2D.OverlapBox(GroundCheckCenter, groundCheckSize, 0f, groundLayer) != null;

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

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(GroundCheckCenter, groundCheckSize);
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
