using UnityEngine;

namespace Metroidvania.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class PlayerHitDeath : MonoBehaviour
    {
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DeadHash = Animator.StringToHash("IsDead");

        [SerializeField] private Animator animator;
        private AnimationSound animationSound;

        public bool IsDead { get; private set; }

        private void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            animationSound = AnimationSound.Attach(animator);
        }

        private void Start() => GameManager.Begin(this);

        public void PlayHit()
        {
            if (!IsDead && animator != null)
            {
                animator.SetTrigger(HitHash);
                animationSound?.PlayAnimationSound(AnimationSound.Impact);
            }
        }

        public void Die()
        {
            if (IsDead) return;
            IsDead = true;

            DisableControl(GetComponent<PlayerDash>());
            DisableControl(GetComponent<PlayerAttack>());
            DisableControl(GetComponent<PlayerJump>());
            DisableControl(GetComponent<PlayerMovement>());

            if (animator != null)
            {
                animator.ResetTrigger(HitHash);
                animator.SetBool(DeadHash, true);
            }
            GameManager.Instance?.PlayerDied(this);
        }

        private static void DisableControl(MonoBehaviour control)
        {
            if (control != null)
                control.enabled = false;
        }
    }
}
