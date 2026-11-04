using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Metroidvania.Gameplay
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(PlayerMotor2D), typeof(PlayerMovement))]
    public sealed class PlayerDash : MonoBehaviour
    {
        private static readonly int IsDashingHash = Animator.StringToHash("IsDashing");

        [SerializeField, Min(0.01f)] private float dashSpeed = 14f;
        [SerializeField, Min(0.01f)] private float dashDuration = 0.16f;
        [SerializeField, Min(0f)] private float dashCooldown = 0.45f;
        [SerializeField] private Animator dashAnimator;

        private PlayerMotor2D motor;
        private PlayerMovement movement;
        private bool dashRequested;
        private float dashEndTime;
        private float nextDashTime;

        private void Awake()
        {
            motor = GetComponent<PlayerMotor2D>();
            movement = GetComponent<PlayerMovement>();
        }

        private void Update()
        {
            if (!motor.IsDashing)
                dashRequested |= ReadDashPressed();
        }

        private void FixedUpdate()
        {
            if (motor.IsDashing && Time.fixedTime >= dashEndTime)
                FinishDash();

            if (!dashRequested) return;
            dashRequested = false;

            if (!movement.isActiveAndEnabled || motor.IsDashing || Time.fixedTime < nextDashTime)
                return;

            float direction = movement.FacingRight ? 1f : -1f;
            motor.BeginDash(direction * dashSpeed);
            dashEndTime = Time.fixedTime + dashDuration;
            if (dashAnimator != null)
                dashAnimator.SetBool(IsDashingHash, true);
        }

        private void OnDisable()
        {
            dashRequested = false;
            if (motor != null && motor.IsDashing)
                FinishDash();
        }

        private void FinishDash()
        {
            motor.EndDash();
            nextDashTime = Time.fixedTime + dashCooldown;
            if (dashAnimator != null)
                dashAnimator.SetBool(IsDashingHash, false);
        }

        private static bool ReadDashPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.leftShiftKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.LeftShift);
#else
            return false;
#endif
        }
    }
}
