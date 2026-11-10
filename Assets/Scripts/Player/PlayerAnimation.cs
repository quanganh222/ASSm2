using UnityEngine;

namespace Metroidvania.Gameplay
{
    // Đưa vận tốc và trạng thái chạm đất vào Animator. Blend Tree dùng Speed cho Idle/Run và VerticalSpeed cho Jump/Fall.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    
    
    public sealed class PlayerAnimation : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        [SerializeField] private Animator animator;
        [SerializeField] private PlayerJump playerJump;
        private Rigidbody2D body;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (animator == null)
            {
                Debug.LogError("PlayerAnimation: Assign the player's Animator.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            animator.SetFloat(SpeedHash, Mathf.Abs(body.linearVelocity.x));
            animator.SetFloat("VerticalSpeed", body.linearVelocity.y);
            animator.SetBool("IsGrounded", playerJump.IsGrounded);
        }
    }
}
