using UnityEngine;

/// <summary>
/// A simple procedural audio manager that generates its own sound effects using sine waves.
/// This prevents the need for external MP3 files while still providing a cozy game feel.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource _source;

    private AudioClip _plopClip;
    private AudioClip _biteClip;
    private AudioClip _alertClip;

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

        GenerateClips();
    }

    private void GenerateClips()
    {
        _plopClip = GenerateTone(400f, 0.1f, 44100, true);
        _biteClip = GenerateTone(800f, 0.05f, 44100, false);
        _alertClip = GenerateTone(220f, 0.5f, 44100, false, 2f); // lower, longer, square-ish
    }

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
}
