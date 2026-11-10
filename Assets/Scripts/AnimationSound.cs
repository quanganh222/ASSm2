using UnityEngine;

namespace Metroidvania.Gameplay
{
    // Đặt bộ nhận Animation Event cùng object với Animator để phát tiếng đúng frame của động tác.
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

        // Tham số của event là vị trí clip trong danh sách âm thanh của GameAudioBank.
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
