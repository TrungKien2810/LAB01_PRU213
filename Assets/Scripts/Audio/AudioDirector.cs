using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>
    /// The game's single audio voice. It creates itself on the first <see cref="Play"/> call and
    /// survives scene loads, so no scene or prefab has to contain it (same trick as
    /// <see cref="SceneLoader"/>). Clips come from <see cref="SoundBank"/>.
    /// </summary>
    public sealed class AudioDirector : MonoBehaviour
    {
        /// <summary>Headroom kept for overlapping effects: clips peak at 1.0, the mix does not.</summary>
        const float MasterGain = 0.7f;

        static AudioDirector instance;
        static float volume = 1f;
        static bool muted;

        AudioSource source;
        AudioClip[] clips;
        float[] lastPlayedAt;

        /// <summary>Master gain for every effect, 0 (silent) to 1 (full).</summary>
        public static float Volume
        {
            get => volume;
            set => volume = Mathf.Clamp01(value);
        }

        /// <summary>Silences the game without touching <see cref="Volume"/>.</summary>
        public static bool Muted
        {
            get => muted;
            set => muted = value;
        }

        /// <summary>Plays an effect, unless muted or a copy of it is still ringing from just now.</summary>
        public static void Play(SoundEffect effect)
        {
            if (muted || volume <= 0f) return;
            if (instance == null) instance = Create();
            instance.PlayNow(effect);
        }

        /// <summary>Supports "Enter Play Mode" without domain reload.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            instance = null;
            volume = 1f;
            muted = false;
        }

        static AudioDirector Create()
        {
            var root = new GameObject(nameof(AudioDirector));
            DontDestroyOnLoad(root);

            var director = root.AddComponent<AudioDirector>();
            director.source = root.AddComponent<AudioSource>();
            director.source.playOnAwake = false;
            director.source.loop = false;
            director.source.spatialBlend = 0f; // 2D: the camera moves, the mixer does not
            director.source.priority = 32;     // default 128 lets the engine steal effects first
            director.clips = SoundBank.CreateAll();

            director.lastPlayedAt = new float[director.clips.Length];
            for (int i = 0; i < director.lastPlayedAt.Length; i++) director.lastPlayedAt[i] = float.NegativeInfinity;
            return director;
        }

        void PlayNow(SoundEffect effect)
        {
            int index = (int)effect;
            if (clips == null || index < 0 || index >= clips.Length) return;

            float now = Time.unscaledTime;
            if (now - lastPlayedAt[index] < MinInterval(effect)) return;
            lastPlayedAt[index] = now;

            if (clips[index]) source.PlayOneShot(clips[index], MasterGain * volume);
        }

        /// <summary>
        /// How long a sound ignores repeat requests. Without it a held fire button would stack
        /// twenty lasers into a wall of noise instead of a rhythm, a boss sitting on the ship
        /// would retrigger its ram fifty times a second, and the End Game screen could start its
        /// jingle twice.
        /// </summary>
        static float MinInterval(SoundEffect effect) => effect switch
        {
            SoundEffect.LaserShot => 0.05f,
            SoundEffect.AsteroidCrack => 0.05f,
            SoundEffect.Explosion => 0.07f,
            SoundEffect.BossHit => 0.06f,
            SoundEffect.ShieldImpact => 0.04f,
            SoundEffect.HullImpact => 0.12f,
            SoundEffect.UiHover => 0.05f,
            SoundEffect.UiClick => 0.05f,
            SoundEffect.PlayerHit => 0.10f,
            SoundEffect.PlayerDeath => 1f,
            SoundEffect.BossWarning => 1f,
            SoundEffect.GameOver => 1.5f,
            SoundEffect.NewRecord => 1.5f,
            _ => 0f,
        };
    }
}
