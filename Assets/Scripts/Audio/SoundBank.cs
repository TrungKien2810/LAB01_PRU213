using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>
    /// Synthesises every sound effect of the game at runtime, so the project needs no binary
    /// audio assets. Each clip is mono at 44.1 kHz and built from a few primitives (a sine wave
    /// with a square edge, low-passed noise) shaped by exponential decay envelopes — the retro
    /// bleeps a space shooter wants. To use recorded audio instead, return the imported
    /// AudioClip from the matching builder method below.
    /// </summary>
    internal static class SoundBank
    {
        const int SampleRate = 44100;
        const float FadeSeconds = 0.003f;
        const float TwoPi = 6.2831853f;

        /// <summary>Own generator, so synthesising clips never disturbs gameplay randomness.</summary>
        static readonly System.Random Rng = new(20240617);

        /// <summary>One clip per <see cref="SoundEffect"/> value, in enum order.</summary>
        internal static AudioClip[] CreateAll() => new[]
        {
            LaserShot(),    // SoundEffect.LaserShot
            AsteroidCrack(),// SoundEffect.AsteroidCrack
            Explosion(),    // SoundEffect.Explosion
            PlayerHit(),    // SoundEffect.PlayerHit
            PlayerDeath(),  // SoundEffect.PlayerDeath
            StarPickup(),   // SoundEffect.StarPickup
            BossWarning(),  // SoundEffect.BossWarning
            BossShot(),     // SoundEffect.BossShot
            BossHit(),      // SoundEffect.BossHit
            BossExplosion(),// SoundEffect.BossExplosion
            UiClick(),      // SoundEffect.UiClick
            NewRecord(),    // SoundEffect.NewRecord
        };

        // ───────────────────────────── the sounds ─────────────────────────────

        /// <summary>A short "pew": two sweeps falling from bright to dark.</summary>
        static AudioClip LaserShot()
        {
            float[] data = Buffer(0.16f);
            Tone(data, 0f, 0.14f, 1250f, 260f, 0.26f, decay: 9f, brightness: 0.45f);
            Tone(data, 0f, 0.10f, 620f, 130f, 0.13f, decay: 13f, brightness: 0.8f);
            return Clip(nameof(SoundEffect.LaserShot), data);
        }

        /// <summary>Rock chipping: a dry crack plus a short low thump.</summary>
        static AudioClip AsteroidCrack()
        {
            float[] data = Buffer(0.14f);
            Noise(data, 0f, 0.12f, 0.30f, decay: 22f, filter: 0.45f);
            Tone(data, 0f, 0.12f, 180f, 70f, 0.24f, decay: 14f, brightness: 0.2f);
            return Clip(nameof(SoundEffect.AsteroidCrack), data);
        }

        /// <summary>Blast: filtered noise for the debris, a falling sine for the punch.</summary>
        static AudioClip Explosion()
        {
            float[] data = Buffer(0.42f);
            Noise(data, 0f, 0.40f, 0.45f, decay: 8f, filter: 0.16f);
            Noise(data, 0f, 0.18f, 0.20f, decay: 20f, filter: 0.5f);
            Tone(data, 0f, 0.34f, 220f, 45f, 0.32f, decay: 7f, brightness: 0.15f);
            return Clip(nameof(SoundEffect.Explosion), data);
        }

        /// <summary>Damage alarm: a rude buzz sliding down, bright enough to cut through.</summary>
        static AudioClip PlayerHit()
        {
            float[] data = Buffer(0.30f);
            Tone(data, 0f, 0.26f, 520f, 110f, 0.26f, decay: 8f, brightness: 0.85f);
            Tone(data, 0f, 0.26f, 260f, 90f, 0.16f, decay: 7f, brightness: 0.6f);
            Noise(data, 0f, 0.20f, 0.16f, decay: 12f, filter: 0.35f);
            return Clip(nameof(SoundEffect.PlayerHit), data);
        }

        /// <summary>Long, deep explosion for the end of a run.</summary>
        static AudioClip PlayerDeath()
        {
            float[] data = Buffer(1.1f);
            Noise(data, 0f, 1.05f, 0.50f, decay: 3.4f, filter: 0.12f);
            Tone(data, 0f, 0.90f, 260f, 35f, 0.36f, decay: 3.2f, brightness: 0.15f);
            Tone(data, 0.05f, 0.85f, 160f, 28f, 0.22f, decay: 2.8f, brightness: 0f);
            return Clip(nameof(SoundEffect.PlayerDeath), data);
        }

        /// <summary>Pickup: three rising blips (C6, E6, G6).</summary>
        static AudioClip StarPickup()
        {
            float[] data = Buffer(0.38f);
            Tone(data, 0f, 0.12f, 1046f, 1046f, 0.22f, decay: 16f, brightness: 0.35f);
            Tone(data, 0.09f, 0.12f, 1318f, 1318f, 0.22f, decay: 16f, brightness: 0.35f);
            Tone(data, 0.18f, 0.18f, 1568f, 1568f, 0.24f, decay: 10f, brightness: 0.35f);
            return Clip(nameof(SoundEffect.StarPickup), data);
        }

        /// <summary>Two-tone alarm, long enough to cover the boss warning banner.</summary>
        static AudioClip BossWarning()
        {
            float[] data = Buffer(2.4f);
            for (int i = 0; i < 6; i++)
            {
                float frequency = i % 2 == 0 ? 740f : 520f;
                Tone(data, i * 0.4f, 0.36f, frequency, frequency, 0.17f, decay: 1.4f, brightness: 0.55f);
            }
            return Clip(nameof(SoundEffect.BossWarning), data);
        }

        /// <summary>Heavier, slower version of the laser: the boss opening fire.</summary>
        static AudioClip BossShot()
        {
            float[] data = Buffer(0.22f);
            Tone(data, 0f, 0.20f, 420f, 120f, 0.28f, decay: 9f, brightness: 0.5f);
            Noise(data, 0f, 0.10f, 0.12f, decay: 16f, filter: 0.3f);
            return Clip(nameof(SoundEffect.BossShot), data);
        }

        /// <summary>Metal-on-shield tick when a laser connects with the boss.</summary>
        static AudioClip BossHit()
        {
            float[] data = Buffer(0.13f);
            Tone(data, 0f, 0.11f, 340f, 180f, 0.20f, decay: 13f, brightness: 0.9f);
            Noise(data, 0f, 0.08f, 0.22f, decay: 24f, filter: 0.55f);
            return Clip(nameof(SoundEffect.BossHit), data);
        }

        /// <summary>The loudest blast of the game: the boss' destruction.</summary>
        static AudioClip BossExplosion()
        {
            float[] data = Buffer(1.5f);
            Noise(data, 0f, 1.45f, 0.55f, decay: 2.6f, filter: 0.12f);
            Noise(data, 0.35f, 0.90f, 0.30f, decay: 2.2f, filter: 0.35f);
            Tone(data, 0f, 1.20f, 300f, 30f, 0.38f, decay: 2.4f, brightness: 0.2f);
            return Clip(nameof(SoundEffect.BossExplosion), data);
        }

        /// <summary>UI tick: a very short blip with a click at the front.</summary>
        static AudioClip UiClick()
        {
            float[] data = Buffer(0.06f);
            Noise(data, 0f, 0.02f, 0.14f, decay: 60f, filter: 0.9f);
            Tone(data, 0f, 0.05f, 1500f, 900f, 0.20f, decay: 30f, brightness: 0.4f);
            return Clip(nameof(SoundEffect.UiClick), data);
        }

        /// <summary>Fanfare for a beaten best score: four rising notes.</summary>
        static AudioClip NewRecord()
        {
            float[] data = Buffer(0.85f);
            float[] notes = { 523f, 659f, 784f, 1046f };
            for (int i = 0; i < notes.Length; i++)
            {
                float length = i == notes.Length - 1 ? 0.45f : 0.14f;
                Tone(data, i * 0.13f, length, notes[i], notes[i], 0.22f, decay: 8f, brightness: 0.3f);
                Tone(data, i * 0.13f, length, notes[i] * 2f, notes[i] * 2f, 0.09f, decay: 10f, brightness: 0.2f);
            }
            return Clip(nameof(SoundEffect.NewRecord), data);
        }

        // ───────────────────────────── synthesis ─────────────────────────────

        static float[] Buffer(float seconds) => new float[Mathf.CeilToInt(seconds * SampleRate)];

        /// <summary>
        /// Adds a wave that sweeps from <paramref name="fromHz"/> to <paramref name="toHz"/> and
        /// fades out exponentially. <paramref name="brightness"/> mixes a square wave into the sine
        /// (0 = pure sine, 1 = full square). Calls are additive, so layers stacked in time make up
        /// one sound.
        /// </summary>
        static void Tone(float[] data, float startSeconds, float durationSeconds, float fromHz, float toHz,
                         float amplitude, float decay, float brightness)
        {
            int first = ToSample(startSeconds);
            int count = Mathf.Min(ToSample(durationSeconds), data.Length - first);
            float phase = 0f;
            for (int n = 0; n < count; n++)
            {
                float progress = (float)n / Mathf.Max(1, count);
                phase += Mathf.Lerp(fromHz, toHz, progress) / SampleRate;
                float wave = Mathf.Sin(phase * TwoPi) * (1f - brightness) + Square(phase) * brightness;
                data[first + n] += wave * amplitude * Decay(decay, n);
            }
        }

        /// <summary>
        /// Adds a burst of noise run through a one-pole low-pass filter: the body of every impact.
        /// <paramref name="filter"/> is how much of each new sample is kept (0 = a deep rumble,
        /// 1 = raw static).
        /// </summary>
        static void Noise(float[] data, float startSeconds, float durationSeconds, float amplitude, float decay,
                          float filter)
        {
            int first = ToSample(startSeconds);
            int count = Mathf.Min(ToSample(durationSeconds), data.Length - first);
            float smoothed = 0f;
            for (int n = 0; n < count; n++)
            {
                float sample = (float)(Rng.NextDouble() * 2.0 - 1.0);
                smoothed += (sample - smoothed) * filter;
                data[first + n] += smoothed * amplitude * Decay(decay, n) * FilterGain(filter);
            }
        }

        static float Square(float phase) => phase % 1f < 0.5f ? 1f : -1f;

        /// <summary>Exponential decay reached <paramref name="decay"/> times per second.</summary>
        static float Decay(float decay, int sample) => Mathf.Exp(-decay * sample / SampleRate);

        /// <summary>A low-passed noise burst is much quieter than raw static; even the levels out.</summary>
        static float FilterGain(float filter) => 1f / Mathf.Sqrt(2f * filter);

        /// <summary>Freezes a buffer into a playable clip, fading the ends so nothing pops.</summary>
        static AudioClip Clip(string name, float[] data)
        {
            int fade = Mathf.Max(1, ToSample(FadeSeconds));
            for (int n = 0; n < fade && n < data.Length; n++)
            {
                float gain = (float)n / fade;
                data[n] *= gain;
                data[data.Length - 1 - n] *= gain;
            }
            for (int n = 0; n < data.Length; n++) data[n] = Mathf.Clamp(data[n], -1f, 1f);

            AudioClip clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static int ToSample(float seconds) => Mathf.Max(0, Mathf.RoundToInt(seconds * SampleRate));
    }
}
