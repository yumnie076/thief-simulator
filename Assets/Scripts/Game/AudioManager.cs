using UnityEngine;

/// <summary>
/// Procedural audio — generates clips from sine/noise waves.
/// No audio files needed. Attach to any persistent GameObject.
/// Call AudioManager.Instance.PlayPickup() / PlayAlarm() / PlayFootstep()
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource _src;

    // Cached clips
    private AudioClip _clipPickup;
    private AudioClip _clipAlarm;
    private AudioClip _clipFootstep;
    private AudioClip _clipHuh;

    private float _footstepTimer;
    private const float FootstepInterval = 0.32f;  // seconds between steps

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _src = gameObject.AddComponent<AudioSource>();
        _src.playOnAwake = false;
        _src.volume = 0.4f;

        // Pre-bake all clips
        _clipPickup   = GenerateTone(880f, 0.08f, fadeOut: true);
        _clipAlarm     = GenerateAlarm();
        _clipFootstep  = GenerateNoise(0.05f, 0.6f);
        _clipHuh       = GenerateTone(330f, 0.15f, fadeOut: true); // Huh?
        
        // Ambient
        var amb = gameObject.AddComponent<AudioSource>();
        amb.clip = GenerateDrone();
        amb.loop = true;
        amb.volume = 0.1f;
        amb.Play();
    }

    private void Update()
    {
        if (GameManager.Instance?.State != GameManager.GameState.Playing) return;

        // Footstep tick — PlayerController drives this via movement check
        var player = FindAnyObjectByType<PlayerController>();
        if (player == null) return;

        bool moving = player.GetComponent<Rigidbody2D>()?.linearVelocity.sqrMagnitude > 0.1f;
        if (!moving) { _footstepTimer = 0; return; }

        _footstepTimer -= Time.deltaTime;
        if (_footstepTimer <= 0f)
        {
            float interval = player.IsSneaking ? FootstepInterval * 1.8f : FootstepInterval;
            _footstepTimer = interval;
            PlayFootstep();
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────
    public void PlayPickup()  => PlayClip(_clipPickup,  0.5f);
    public void PlayAlarm()   => PlayClip(_clipAlarm,   0.8f);
    public void PlayFootstep()=> PlayClip(_clipFootstep, player: true);
    public void PlayHuh()     => PlayClip(_clipHuh,     0.6f);

    private void PlayClip(AudioClip clip, float volume = 0.4f, bool player = false)
    {
        if (clip == null || _src == null) return;
        _src.volume = volume;
        _src.PlayOneShot(clip);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Procedural clip generators
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Single sine-wave tone.</summary>
    static AudioClip GenerateTone(float freq, float duration, bool fadeOut = false)
    {
        int   rate    = 44100;
        int   samples = Mathf.RoundToInt(rate * duration);
        float[] data  = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t    = (float)i / rate;
            float env  = fadeOut ? (1f - (float)i / samples) : 1f;
            data[i]    = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.6f;
        }
        var clip = AudioClip.Create("Tone", samples, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>Two-tone alarm sweep (low-high alternating).</summary>
    static AudioClip GenerateAlarm()
    {
        int   rate    = 44100;
        int   samples = rate;   // 1 second
        float[] data  = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t    = (float)i / rate;
            float freq = (t % 0.5f < 0.25f) ? 440f : 660f;
            data[i]    = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f;
        }
        var clip = AudioClip.Create("Alarm", samples, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>Short white-noise burst for footstep.</summary>
    static AudioClip GenerateNoise(float duration, float filterFreq)
    {
        int   rate    = 44100;
        int   samples = Mathf.RoundToInt(rate * duration);
        float[] data  = new float[samples];
        float prev    = 0f;
        for (int i = 0; i < samples; i++)
        {
            float noise  = Random.Range(-1f, 1f);
            // Simple one-pole low-pass
            float rc     = 1f / (2f * Mathf.PI * filterFreq);
            float dt     = 1f / rate;
            float alpha  = dt / (rc + dt);
            prev         = prev + alpha * (noise - prev);
            float env    = 1f - (float)i / samples;
            data[i]      = prev * env * 0.3f;
        }
        var clip = AudioClip.Create("Footstep", samples, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>Low frequency drone for atmosphere.</summary>
    static AudioClip GenerateDrone()
    {
        int rate = 44100;
        int samples = rate * 2; // 2 seconds loop
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / rate;
            // Mixed sine waves for a rich low hum
            data[i] = (Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.5f +
                       Mathf.Sin(2f * Mathf.PI * 90f * t) * 0.3f +
                       Mathf.Sin(2f * Mathf.PI * 40f * t) * 0.2f) * 0.5f;
        }
        var clip = AudioClip.Create("Drone", samples, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
