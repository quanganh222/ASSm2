using UnityEngine;

namespace Metroidvania.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class AnimationSound : MonoBehaviour
    {
        internal const int Impact = 4;

        private AudioSource source;

        internal static AnimationSound Attach(Animator animator)
        {
            if (animator == null) return null;
            if (animator.TryGetComponent<AnimationSound>(out var existing)) return existing;

            return animator.gameObject.AddComponent<AnimationSound>();
        }

        // Animation Events pass the slot in GameAudioBank.
        public void PlayAnimationSound(int cue)
        {
            var bank = GameAudioBank.Current;
            if (bank == null) return;

            if (source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
            }
            bank.PlayCue(source, cue);
        }
    }
}
