using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Metroidvania.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerJump), typeof(PlayerMotor2D))]
    public sealed class PlayerAttack : MonoBehaviour
    {
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int AttackTypeHash = Animator.StringToHash("AttackType");
        private static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
        private static readonly int JumpingHash = Animator.StringToHash("Jumping");

        [SerializeField] private Animator animator;

        private PlayerJump playerJump;
        private PlayerMotor2D motor;
        private int nextGroundAttack;

        private void Awake()
        {
            playerJump = GetComponent<PlayerJump>();
            motor = GetComponent<PlayerMotor2D>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (animator == null)
            {
                Debug.LogError("PlayerAttack: Assign the player's Animator.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (!ReadAttackPressed() || motor.IsDashing || animator.IsInTransition(0))
                return;

            int state = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            if (state != LocomotionHash && state != JumpingHash)
                return;

            bool isGrounded = playerJump.IsGrounded;
            animator.SetFloat(AttackTypeHash, isGrounded ? nextGroundAttack : 2f);
            animator.SetTrigger(AttackHash);

            if (isGrounded)
                nextGroundAttack = 1 - nextGroundAttack;
        }

        private static bool ReadAttackPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            return mouse != null && mouse.leftButton.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(0);
#else
            return false;
#endif
        }
    }
}
