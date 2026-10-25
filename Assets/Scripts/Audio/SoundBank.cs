using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>
    /// Synthesises every sound effect of the game at runtime, so the project needs no binary
    /// audio assets. Each clip is mono at 44.1 kHz and built from a few primitives (a sine wave
    /// with a square edge, low-passed noise) shaped by exponential decay envelopes — the retro
    /// bleeps a space shooter wants. To use recorded audio instead, return the imported
    /// AudioClip from the matching builder method below.
    ///
    /// Two rules keep the mix readable: every effect owns a distinct register and shape (so no
    /// two events sound alike), and <see cref="Clip"/> normalises each one to an explicit peak
    /// level, so a laser never drowns a menu tick and nothing clips on the way out.
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
            UiHover(),      // SoundEffect.UiHover
            ShieldImpact(), // SoundEffect.ShieldImpact
            HullImpact(),   // SoundEffect.HullImpact
            GameOver(),     // SoundEffect.GameOver
        };

        // ───────────────────────────── the sounds ─────────────────────────────

        /// <summary>
        /// Bright, high and short: the player's "pew". Its snap-and-fall shape is the
        /// opposite of the boss' dark orb launch, so the two are never confused.
        /// </summary>
        static AudioClip LaserShot()
        {
            float[] data = Buffer(0.16f);
            Noise(data, 0f, 0.02f, 0.30f, decay: 120f, filter: 0.85f); // photon snap at the front
            Tone(data, 0f, 0.13f, 1400f, 300f, 0.50f, decay: 12f, brightness: 0.30f);
            Tone(data, 0f, 0.10f, 700f, 160f, 0.28f, decay: 16f, brightness: 0.55f);
            return Clip(nameof(SoundEffect.LaserShot), data, peak: 0.72f);
        }

        /// <summary>
        /// Rock chipping: a bright snap over a short mid knock. Deliberately all treble and no
        /// weight, so it reads as chipped stone next to the bass of <see cref="Explosion"/> and
        /// the ringing metal of <see cref="BossHit"/>.
        /// </summary>
        static AudioClip AsteroidCrack()
        {
            float[] data = Buffer(0.12f);
            Noise(data, 0f, 0.035f, 0.45f, decay: 90f, filter: 0.80f);
            Noise(data, 0f, 0.09f, 0.55f, decay: 34f, filter: 0.55f);
            Tone(data, 0f, 0.08f, 820f, 430f, 0.45f, decay: 26f, brightness: 0.25f);
            return Clip(nameof(SoundEffect.AsteroidCrack), data, peak: 0.68f);
        }

        /// <summary>Blast: filtered noise for the debris, a falling sine for the punch.</summary>
        static AudioClip Explosion()
        {
            float[] data = Buffer(0.42f);
            Noise(data, 0f, 0.40f, 0.60f, decay: 8f, filter: 0.16f);
            Noise(data, 0f, 0.16f, 0.30f, decay: 24f, filter: 0.55f);
            Tone(data, 0f, 0.34f, 240f, 45f, 0.40f, decay: 7f, brightness: 0.10f);
            return Clip(nameof(SoundEffect.Explosion), data, peak: 0.85f);
        }

        /// <summary>
        /// The "life lost" alarm: square-edged and mid-range, so it stays recognisable when it
        /// lands on top of the impact that caused it.
        /// </summary>
        static AudioClip PlayerHit()
        {
            float[] data = Buffer(0.26f);
            Tone(data, 0f, 0.22f, 440f, 150f, 0.45f, decay: 11f, brightness: 0.85f);
            Tone(data, 0f, 0.24f, 175f, 120f, 0.30f, decay: 8f, brightness: 0.65f);
            return Clip(nameof(SoundEffect.PlayerHit), data, peak: 0.70f);
        }

        /// <summary>Long, deep explosion for the end of a run.</summary>
        static AudioClip PlayerDeath()
        {
            float[] data = Buffer(1.2f);
            Noise(data, 0f, 1.10f, 0.62f, decay: 3.2f, filter: 0.12f);
            Noise(data, 0f, 0.30f, 0.35f, decay: 16f, filter: 0.45f);
            Tone(data, 0f, 0.95f, 280f, 32f, 0.45f, decay: 3.0f, brightness: 0.12f);
            Tone(data, 0.06f, 0.80f, 150f, 26f, 0.28f, decay: 2.6f, brightness: 0f);
            return Clip(nameof(SoundEffect.PlayerDeath), data, peak: 0.92f);
        }

        /// <summary>Pickup: three rising blips (C6, E6, G6) with a shimmer on the last one.</summary>
        static AudioClip StarPickup()
        {
            float[] data = Buffer(0.40f);
            Tone(data, 0f, 0.11f, 1046f, 1046f, 0.35f, decay: 20f, brightness: 0.20f);
            Tone(data, 0.09f, 0.11f, 1318f, 1318f, 0.35f, decay: 20f, brightness: 0.20f);
            Tone(data, 0.18f, 0.20f, 1568f, 1568f, 0.40f, decay: 11f, brightness: 0.20f);
            Tone(data, 0.18f, 0.20f, 3136f, 3136f, 0.12f, decay: 14f, brightness: 0f);
            return Clip(nameof(SoundEffect.StarPickup), data, peak: 0.58f);
        }

        /// <summary>Two-tone alarm, long enough to cover the boss warning banner.</summary>
        static AudioClip BossWarning()
        {
            float[] data = Buffer(2.4f);
            for (int i = 0; i < 6; i++)
            {
                float frequency = i % 2 == 0 ? 780f : 520f;
                Tone(data, i * 0.40f, 0.34f, frequency, frequency * 0.94f, 0.40f, decay: 1.2f, brightness: 0.45f);
                Tone(data, i * 0.40f, 0.34f, frequency * 0.5f, frequency * 0.47f, 0.22f, decay: 1.2f, brightness: 0.20f);
            }
            return Clip(nameof(SoundEffect.BossWarning), data, peak: 0.62f);
        }

        /// <summary>
        /// The boss opening fire: a heavy, dark orb launch with almost no high end, sitting a
        /// full octave below the player's laser.
        /// </summary>
        static AudioClip BossShot()
        {
            float[] data = Buffer(0.24f);
            Tone(data, 0f, 0.22f, 320f, 85f, 0.55f, decay: 8f, brightness: 0.12f);
            Tone(data, 0f, 0.24f, 160f, 55f, 0.35f, decay: 6.5f, brightness: 0f);
            Noise(data, 0f, 0.08f, 0.20f, decay: 20f, filter: 0.25f);
            return Clip(nameof(SoundEffect.BossShot), data, peak: 0.80f);
        }

        /// <summary>
        /// Metal-on-shield ping: two nearly matched high partials beat against each other, which
        /// reads as struck hull rather than as the dry rock of <see cref="AsteroidCrack"/>.
        /// </summary>
        static AudioClip BossHit()
        {
            float[] data = Buffer(0.16f);
            Metal(data, 0f, 0.15f, 900f, 0.45f, decay: 22f);
            Noise(data, 0f, 0.05f, 0.28f, decay: 60f, filter: 0.75f);
            return Clip(nameof(SoundEffect.BossHit), data, peak: 0.62f);
        }

        /// <summary>The loudest blast of the game: the boss' destruction.</summary>
        static AudioClip BossExplosion()
        {
            float[] data = Buffer(1.6f);
            Noise(data, 0f, 1.50f, 0.70f, decay: 2.5f, filter: 0.12f);
            Noise(data, 0.32f, 0.95f, 0.42f, decay: 2.0f, filter: 0.32f);
            Noise(data, 0f, 0.25f, 0.40f, decay: 14f, filter: 0.50f);
            Tone(data, 0f, 1.25f, 320f, 28f, 0.50f, decay: 2.3f, brightness: 0.15f);
            return Clip(nameof(SoundEffect.BossExplosion), data, peak: 0.95f);
        }

        /// <summary>
        /// UI confirm: a low body with a rising blip on top — two steps, so a press is clearly a
        /// different event from merely moving onto a button (<see cref="UiHover"/>) and cannot be
        /// mistaken for it.
        /// </summary>
        static AudioClip UiClick()
        {
            float[] data = Buffer(0.11f);
            Noise(data, 0f, 0.012f, 0.20f, decay: 150f, filter: 0.90f);
            Tone(data, 0f, 0.035f, 520f, 430f, 0.42f, decay: 62f, brightness: 0.45f);
            Tone(data, 0.030f, 0.06f, 1000f, 1180f, 0.38f, decay: 42f, brightness: 0.30f);
            return Clip(nameof(SoundEffect.UiClick), data, peak: 0.55f);
        }

        /// <summary>Fanfare for a beaten best score: four rising notes.</summary>
        static AudioClip NewRecord()
        {
            float[] data = Buffer(0.95f);
            float[] notes = { 523f, 659f, 784f, 1046f };
            for (int i = 0; i < notes.Length; i++)
            {
                float length = i == notes.Length - 1 ? 0.50f : 0.14f;
                Tone(data, i * 0.13f, length, notes[i], notes[i], 0.40f, decay: 8f, brightness: 0.30f);
                Tone(data, i * 0.13f, length, notes[i] * 2f, notes[i] * 2f, 0.16f, decay: 10f, brightness: 0.15f);
            }
            return Clip(nameof(SoundEffect.NewRecord), data, peak: 0.72f);
        }

        /// <summary>
        /// Moving onto a button: one very short high blip, deliberately much quieter than
        /// <see cref="UiClick"/> so a menu full of buttons never turns into noise.
        /// </summary>
        static AudioClip UiHover()
        {
            float[] data = Buffer(0.05f);
            Tone(data, 0f, 0.04f, 1568f, 1760f, 0.60f, decay: 55f, brightness: 0.15f);
            return Clip(nameof(SoundEffect.UiHover), data, peak: 0.30f);
        }

        /// <summary>
        /// An energy round bursting on the ship's shield: a glassy high snap that decays fast.
        /// The asteroid ram (<see cref="HullImpact"/>) is its opposite — deep and heavy.
        /// </summary>
        static AudioClip ShieldImpact()
        {
            float[] data = Buffer(0.14f);
            Noise(data, 0f, 0.05f, 0.55f, decay: 80f, filter: 0.88f);
            Tone(data, 0f, 0.09f, 2400f, 800f, 0.45f, decay: 34f, brightness: 0.20f);
            Tone(data, 0f, 0.13f, 1200f, 360f, 0.30f, decay: 26f, brightness: 0.30f);
            return Clip(nameof(SoundEffect.ShieldImpact), data, peak: 0.70f);
        }

        /// <summary>
        /// Rock or hull slamming into the ship: a deep thud under a short crack, so a ram is
        /// felt as weight rather than heard as a zap.
        /// </summary>
        static AudioClip HullImpact()
        {
            float[] data = Buffer(0.24f);
            Noise(data, 0f, 0.06f, 0.60f, decay: 70f, filter: 0.75f);
            Noise(data, 0f, 0.20f, 0.55f, decay: 15f, filter: 0.22f);
            Tone(data, 0f, 0.22f, 120f, 42f, 0.60f, decay: 9f, brightness: 0.08f);
            Tone(data, 0f, 0.14f, 420f, 260f, 0.20f, decay: 22f, brightness: 0.50f);
            return Clip(nameof(SoundEffect.HullImpact), data, peak: 0.88f);
        }

        /// <summary>
        /// End of a lost run: four descending minor notes over a dark rumble. The mirror image
        /// of the rising <see cref="NewRecord"/> fanfare, so the screen's verdict is audible
        /// before it is read.
        /// </summary>
        static AudioClip GameOver()
        {
            float[] data = Buffer(1.9f);
            Noise(data, 0f, 1.70f, 0.28f, decay: 1.6f, filter: 0.08f);
            float[] notes = { 440f, 349.23f, 293.66f, 220f }; // A4, F4, D4, A3 — A minor falling
            for (int i = 0; i < notes.Length; i++)
            {
                bool last = i == notes.Length - 1;
                float length = last ? 0.85f : 0.30f;
                Tone(data, i * 0.30f, length, notes[i], notes[i] * 0.995f, 0.34f, decay: last ? 2.5f : 6f, brightness: 0.25f);
                Tone(data, i * 0.30f, length, notes[i] * 0.5f, notes[i] * 0.5f, 0.22f, decay: last ? 2.5f : 4f, brightness: 0f);
            }
            return Clip(nameof(SoundEffect.GameOver), data, peak: 0.80f);
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
        /// Adds a struck-metal ring: two close partials that beat against each other, plus a
        /// quieter inharmonic partial for the edge. Reads as metal where a plain sine reads as a beep.
        /// </summary>
        static void Metal(float[] data, float startSeconds, float durationSeconds, float hz,
                          float amplitude, float decay)
        {
            Tone(data, startSeconds, durationSeconds, hz, hz * 0.97f, amplitude, decay, brightness: 0.55f);
            Tone(data, startSeconds, durationSeconds, hz * 1.51f, hz * 1.47f, amplitude * 0.55f, decay * 1.3f, brightness: 0.45f);
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

        /// <summary>
        /// Freezes a buffer into a playable clip. Fades the ends so nothing pops, then scales the
        /// whole clip to <paramref name="peak"/>: the loudness of every effect is decided here, in
        /// one place, instead of being an accident of how many layers happen to overlap.
        /// </summary>
        static AudioClip Clip(string name, float[] data, float peak)
        {
            int fade = Mathf.Max(1, ToSample(FadeSeconds));
            for (int n = 0; n < fade && n < data.Length; n++)
            {
                float gain = (float)n / fade;
                data[n] *= gain;
                data[data.Length - 1 - n] *= gain;
            }

            float loudest = 0f;
            for (int n = 0; n < data.Length; n++) loudest = Mathf.Max(loudest, Mathf.Abs(data[n]));

            float scale = loudest > 0.0001f ? Mathf.Min(peak / loudest, 8f) : 0f;
            for (int n = 0; n < data.Length; n++) data[n] = Mathf.Clamp(data[n] * scale, -1f, 1f);

            AudioClip clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static int ToSample(float seconds) => Mathf.Max(0, Mathf.RoundToInt(seconds * SampleRate));
    }
}
