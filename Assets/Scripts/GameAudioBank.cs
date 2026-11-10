using UnityEngine;

namespace Metroidvania.Gameplay
{
    public sealed class GameAudioBank : ScriptableObject
    {
        [SerializeField] private AudioClip music;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.26f;
        [SerializeField, Range(0f, 1f)] private float effectsVolume = 0.55f;
        [SerializeField, Tooltip("Cue slots: Footstep, Jump, Dash, Slash, Impact, Death, Shot, Explosion, Laser, Web.")]
        private AudioClip[] animationCues;

        private static AudioSource musicSource;
        private static GameAudioBank current;
        internal static GameAudioBank Current => current != null
            ? current
            : current = Resources.Load<GameAudioBank>("GameAudio");

        internal void EnsureMusicPlaying()
        {
            if (musicSource != null || music == null) return;

            var owner = new GameObject("Background Music");
            DontDestroyOnLoad(owner);
            musicSource = owner.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.clip = music;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }

        internal void PlayCue(AudioSource source, int cue)
        {
            if (animationCues != null && cue >= 0 && cue < animationCues.Length && animationCues[cue] != null)
                source.PlayOneShot(animationCues[cue], effectsVolume);
        }
    }
}
