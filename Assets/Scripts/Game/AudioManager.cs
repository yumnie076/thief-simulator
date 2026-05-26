using UnityEngine;

/// <summary>
/// A simple procedural audio manager that generates its own sound effects using sine waves.
/// This prevents the need for external MP3 files while still providing a cozy game feel.
/// Supports one-shot SFX (_source) and looping ambient tracks (_ambientSource).
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource _source;
    private AudioSource _ambientSource;

    // --- One-shot clips ---
    private AudioClip _plopClip;
    private AudioClip _biteClip;
    private AudioClip _alertClip;
    private AudioClip _splashClip;
    private AudioClip _crunchClip;

    // --- Ambient loop clips ---
    private AudioClip _dayAmbienceClip;
    private AudioClip _nightAmbienceClip;

    private const int SampleRate = 44100;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _source = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;

        _ambientSource = gameObject.AddComponent<AudioSource>();
        _ambientSource.playOnAwake = false;
        _ambientSource.loop = true;
        _ambientSource.volume = 0.3f;

        GenerateClips();
    }

    private void GenerateClips()
    {
        // Existing one-shot clips
        _plopClip = GenerateTone(400f, 0.1f, SampleRate, true);
        _biteClip = GenerateTone(800f, 0.05f, SampleRate, false);
        _alertClip = GenerateTone(220f, 0.5f, SampleRate, false, 2f); // lower, longer, square-ish

        // New one-shot clips
        _splashClip = GenerateTone(300f, 0.15f, SampleRate, true, 1.5f);
        _crunchClip = GenerateCrunch();

        // Ambient loops
        _dayAmbienceClip = GenerateBirdAmbience();
        _nightAmbienceClip = GenerateCricketAmbience();
    }

    // ========================
    //  Existing SFX (unchanged)
    // ========================

    public void PlayPlop()
    {
        if (_plopClip != null) _source.PlayOneShot(_plopClip, 0.8f);
    }

    public void PlayBite()
    {
        if (_biteClip != null) _source.PlayOneShot(_biteClip, 0.6f);
    }

    public void PlayAlert()
    {
        if (_alertClip != null) _source.PlayOneShot(_alertClip, 0.4f);
    }

    public void PlaySplash()
    {
        if (_splashClip != null) _source.PlayOneShot(_splashClip, 0.5f);
    }

    // ========================
    //  New one-shot SFX
    // ========================

    public void PlayCrunch()
    {
        if (_crunchClip != null) _source.PlayOneShot(_crunchClip, 0.6f);
    }

    // ========================
    //  Ambient loop controls
    // ========================

    public void StartDayAmbience()
    {
        if (_dayAmbienceClip == null) return;
        _ambientSource.clip = _dayAmbienceClip;
        _ambientSource.Play();
    }

    public void StopDayAmbience()
    {
        if (_ambientSource.isPlaying && _ambientSource.clip == _dayAmbienceClip)
        {
            _ambientSource.Stop();
        }
    }

    public void StartNightAmbience()
    {
        if (_nightAmbienceClip == null) return;
        _ambientSource.clip = _nightAmbienceClip;
        _ambientSource.Play();
    }

    public void StopNightAmbience()
    {
        if (_ambientSource.isPlaying && _ambientSource.clip == _nightAmbienceClip)
        {
            _ambientSource.Stop();
        }
    }

    // ========================
    //  Procedural generators
    // ========================

    /// <summary>
    /// Generates a simple audio clip mathematically.
    /// </summary>
    private AudioClip GenerateTone(float frequency, float duration, int sampleRate, bool slideDown, float harmonic = 1f)
    {
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float currentFreq = frequency;
            if (slideDown)
            {
                currentFreq = Mathf.Lerp(frequency, frequency * 0.5f, t / duration);
            }

            // Mix sine wave with a bit of square for texture
            float sine = Mathf.Sin(2 * Mathf.PI * currentFreq * t);
            float sq = Mathf.Sign(Mathf.Sin(2 * Mathf.PI * currentFreq * harmonic * t)) * 0.2f;

            // Envelope (fade out)
            float envelope = 1f - (t / duration);

            data[i] = (sine + sq) * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create("GeneratedTone", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// Gentle bird chirping: short sine bursts at randomized intervals with varied pitches.
    /// Loops seamlessly for garden/day phase.
    /// </summary>
    private AudioClip GenerateBirdAmbience()
    {
        float duration = 6f; // 6 second loop
        int samples = (int)(SampleRate * duration);
        float[] data = new float[samples];

        // Use a fixed seed so the clip is deterministic across runs
        System.Random rng = new System.Random(42);

        // Pre-generate chirp events: (startSample, frequency, chirpLength)
        int chirpCount = 18;
        for (int c = 0; c < chirpCount; c++)
        {
            float startTime = (float)(rng.NextDouble() * (duration - 0.15f));
            int startSample = (int)(startTime * SampleRate);

            float baseFreq = 2200f + (float)(rng.NextDouble() * 1800f); // 2200-4000 Hz
            float chirpDur = 0.04f + (float)(rng.NextDouble() * 0.08f); // 40-120ms
            int chirpSamples = (int)(chirpDur * SampleRate);

            for (int i = 0; i < chirpSamples; i++)
            {
                int idx = startSample + i;
                if (idx >= samples) break;

                float t = (float)i / SampleRate;
                float progress = (float)i / chirpSamples;

                // Quick attack, smooth decay envelope
                float env = progress < 0.15f
                    ? progress / 0.15f
                    : 1f - ((progress - 0.15f) / 0.85f);
                env *= env; // Make decay more natural

                // Slight frequency sweep upward (bird-like)
                float freq = baseFreq + baseFreq * 0.15f * progress;
                float sample = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.25f;

                // Add a faint harmonic
                sample += Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * env * 0.08f;

                data[idx] += sample;
            }
        }

        // Clamp to prevent any clipping
        for (int i = 0; i < samples; i++)
        {
            data[i] = Mathf.Clamp(data[i], -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("BirdAmbience", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// Cricket ambience: rhythmic high-frequency pulsing for the hedgehog night phase.
    /// </summary>
    private AudioClip GenerateCricketAmbience()
    {
        float duration = 4f; // 4 second loop
        int samples = (int)(SampleRate * duration);
        float[] data = new float[samples];

        // Cricket 1: steady pulse
        float cricketFreq1 = 4800f;
        float pulseRate1 = 14f; // pulses per second

        // Cricket 2: slightly offset
        float cricketFreq2 = 5200f;
        float pulseRate2 = 11f;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;

            // Pulse envelope using a rectified sine
            float pulse1 = Mathf.Max(0f, Mathf.Sin(2f * Mathf.PI * pulseRate1 * t));
            pulse1 *= pulse1; // Sharpen the pulse
            float wave1 = Mathf.Sin(2f * Mathf.PI * cricketFreq1 * t) * pulse1 * 0.15f;

            float pulse2 = Mathf.Max(0f, Mathf.Sin(2f * Mathf.PI * pulseRate2 * t + 1.5f));
            pulse2 *= pulse2;
            float wave2 = Mathf.Sin(2f * Mathf.PI * cricketFreq2 * t) * pulse2 * 0.12f;

            data[i] = wave1 + wave2;
        }

        AudioClip clip = AudioClip.Create("CricketAmbience", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// Splash sound: low-frequency burst mixed with noise for a water-like effect.
    /// </summary>
    private AudioClip GenerateSplash()
    {
        float duration = 0.35f;
        int samples = (int)(SampleRate * duration);
        float[] data = new float[samples];

        System.Random rng = new System.Random(99);

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float progress = t / duration;

            // Fast attack, medium decay envelope
            float env = progress < 0.05f
                ? progress / 0.05f
                : Mathf.Pow(1f - ((progress - 0.05f) / 0.95f), 2f);

            // Low frequency thump
            float low = Mathf.Sin(2f * Mathf.PI * 120f * t) * 0.5f;
            // Mid splash frequencies
            float mid = Mathf.Sin(2f * Mathf.PI * 280f * t) * 0.3f;
            // White noise for water texture
            float noise = ((float)rng.NextDouble() * 2f - 1f) * 0.4f;

            // Noise gets louder as low frequencies fade
            float noiseMix = progress * 0.6f + 0.4f;
            data[i] = (low * (1f - progress) + mid * 0.5f + noise * noiseMix) * env * 0.6f;
        }

        AudioClip clip = AudioClip.Create("Splash", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// Crunch sound: short burst mixing multiple frequencies with noise for a crunchy texture.
    /// </summary>
    private AudioClip GenerateCrunch()
    {
        float duration = 0.08f;
        int samples = (int)(SampleRate * duration);
        float[] data = new float[samples];

        System.Random rng = new System.Random(77);

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float progress = t / duration;

            // Sharp attack, quick decay
            float env = progress < 0.1f
                ? progress / 0.1f
                : Mathf.Pow(1f - ((progress - 0.1f) / 0.9f), 3f);

            // Multiple harsh frequencies for crunch texture
            float f1 = Mathf.Sin(2f * Mathf.PI * 600f * t) * 0.3f;
            float f2 = Mathf.Sin(2f * Mathf.PI * 1500f * t) * 0.25f;
            float f3 = Mathf.Sin(2f * Mathf.PI * 3200f * t) * 0.15f;

            // Noise for grit
            float noise = ((float)rng.NextDouble() * 2f - 1f) * 0.5f;

            data[i] = (f1 + f2 + f3 + noise) * env * 0.5f;
        }

        AudioClip clip = AudioClip.Create("Crunch", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
