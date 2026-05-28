using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton that controls the 4-phase game loop for Egel op Expeditie.
/// Intro → GardenBuild → HedgehogVisit → Result
/// </summary>
public class PhaseController : MonoBehaviour
{
    public static PhaseController Instance { get; private set; }

    // ── Phase enum ──────────────────────────────────────────────
    public enum GamePhase { Intro, GardenBuild, HedgehogVisit, Result }

    // ── State ───────────────────────────────────────────────────
    public GamePhase CurrentPhase { get; private set; } = GamePhase.Intro;

    // Status tracking for win/loss
    public string GameOverReason { get; private set; } = null;
    public bool IsRaining { get; private set; } = false;

    /// <summary>Fired every time the phase changes.</summary>
    public event Action<GamePhase> OnPhaseChanged;

    // ── Inspector refs ──────────────────────────────────────────
    [Header("Fade Overlay")]
    [Tooltip("Full-screen CanvasGroup used for fade-to-black transitions.")]
    [SerializeField] private CanvasGroup fadeOverlay;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    // ── Singleton setup ─────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Ensure overlay starts fully transparent
        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
        }

        // Begin with the intro phase
        StartPhase(GamePhase.Intro);
    }

    // ── Public API ──────────────────────────────────────────────

    /// <summary>
    /// Called by IntroScreenUI buttons to set the garden difficulty.
    /// 0 = hard (mostly paved), 1 = medium (mix), 2 = easy (lots of green).
    /// </summary>
    public void SetGardenStartState(int state)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GardenStartState = Mathf.Clamp(state, 0, 2);

            // Store the level goal text for display during build phase
            switch (state)
            {
                case 0: GameManager.Instance.LevelGoalText = "Verwijder al het afval en plaats minimaal 1 vijver"; break;
                case 1: GameManager.Instance.LevelGoalText = "Bouw een tuin met water, schuilplek en bloemen"; break;
                case 2: GameManager.Instance.LevelGoalText = "Maximaliseer de biodiversiteit!"; break;
            }
        }
    }

    public bool IsTransitioning { get; private set; } = false;

    /// <summary>
    /// Transition to a new phase with a fade effect.
    /// </summary>
    public void StartPhase(GamePhase phase, bool force = false)
    {
        if ((IsTransitioning && !force) || CurrentPhase == phase) return;
        IsTransitioning = true;

        StartCoroutine(FadeTransition(() =>
        {
            CurrentPhase = phase;
            HandlePhaseEntry(phase);
            OnPhaseChanged?.Invoke(phase);
        }));
    }

    // ── Phase entry logic ───────────────────────────────────────

    private void HandlePhaseEntry(GamePhase phase)
    {
        // Handle Audio
        if (AudioManager.Instance != null)
        {
            if (phase == GamePhase.GardenBuild) AudioManager.Instance.StartDayAmbience();
            else AudioManager.Instance.StopDayAmbience();

            if (phase == GamePhase.HedgehogVisit) AudioManager.Instance.StartNightAmbience();
            else AudioManager.Instance.StopNightAmbience();
        }

        switch (phase)
        {
            case GamePhase.Intro:
                // UI will show itself via OnPhaseChanged
                break;

            case GamePhase.GardenBuild:
                InitializeGarden();
                if (ScoreManager.Instance != null)
                    ScoreManager.Instance.ResetScores();
                break;

            case GamePhase.HedgehogVisit:
                SpawnHedgehog();
                break;

            case GamePhase.Result:
                CalculateFinalScore();
                break;
        }
    }

    private void InitializeGarden()
    {
        // GardenManager subscribes to OnPhaseChanged and builds the grid itself.
        Debug.Log($"[PhaseController] Garden phase starting. Start state: {GameManager.Instance?.GardenStartState}");

        // Cleanup NightOverlay if it exists from a previous run
        var night = GameObject.Find("NightOverlay");
        if (night != null) UnityEngine.Object.Destroy(night);
    }

    private void SpawnHedgehog()
    {
        // Destroy gardener player if any
        var players = UnityEngine.Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var p in players)
        {
            UnityEngine.Object.Destroy(p.gameObject);
        }

        // Destroy previous hedgehog/fox if any
        var existing = UnityEngine.Object.FindAnyObjectByType<HedgehogAI>();
        if (existing != null) UnityEngine.Object.Destroy(existing.gameObject);

        var existingFox = UnityEngine.Object.FindAnyObjectByType<FoxAI>();
        if (existingFox != null) UnityEngine.Object.Destroy(existingFox.gameObject);
        
        var existingMower = UnityEngine.Object.FindAnyObjectByType<RobotMowerAI>();
        if (existingMower != null) UnityEngine.Object.Destroy(existingMower.gameObject);

        // Reset win/loss status
        GameOverReason = null;
        IsRaining = false;

        // Create hedgehog GameObject
        var hedgehogGO = new GameObject("Hedgehog");
        var sr = hedgehogGO.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 50; // Ensure visible

        // Try to load the beautifully generated procedural hedgehog sprite
        var hedgehogSprite = Resources.Load<Sprite>("EgelGame/hedgehog");
        if (hedgehogSprite == null)
        {
            // Fallback: create a small brown circle
            var tex = new Texture2D(16, 16);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    float dx = x - 7.5f, dy = y - 7.5f;
                    tex.SetPixel(x, y, dx*dx + dy*dy <= 49 ?
                        new Color(0.42f, 0.26f, 0.15f) : Color.clear);
                }
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0,0,16,16), new Vector2(0.5f,0.5f), 16f);
        }
        else
        {
            sr.sprite = hedgehogSprite;
        }

        hedgehogGO.AddComponent<HedgehogAI>();

        // Position at bottom center of map
        if (GardenManager.Instance != null)
        {
            hedgehogGO.transform.position = new Vector3(
                GardenManager.Instance.GardenWidth / 2f, 
                2f, 0f);
        }

        // Set camera target to track the hedgehog!
        var cam = Camera.main;
        if (cam != null)
        {
            var cf = cam.GetComponent<CameraFollow>();
            if (cf != null) cf.target = hedgehogGO.transform;
        }

        // Spawn Fox Predator at top-left edge
        SpawnFox();

        // Spawn Robot Mower at bottom-right edge
        SpawnMower();

        // Spawn snacks around the garden
        if (GardenManager.Instance != null)
        {
            for (int i = 0; i < 5; i++)
            {
                var snack = new GameObject("Snack");
                float rx = UnityEngine.Random.Range(2f, GardenManager.Instance.GardenWidth - 2f);
                float ry = UnityEngine.Random.Range(2f, GardenManager.Instance.GardenHeight - 2f);
                snack.transform.position = new Vector3(rx, ry, 0f);
                snack.AddComponent<Collectible>();
            }

            // Spawn a friend hedgehog somewhere in the garden
            var friendGO = new GameObject("FriendHedgehog");
            var friendSR = friendGO.AddComponent<SpriteRenderer>();
            friendSR.sortingOrder = 49;

            var friendSprite = Resources.Load<Sprite>("EgelGame/hedgehog");
            if (friendSprite != null)
            {
                friendSR.sprite = friendSprite;
            }
            else
            {
                // Fallback: small brown circle (same as main hedgehog)
                var fTex = new Texture2D(16, 16);
                for (int y = 0; y < 16; y++)
                    for (int x = 0; x < 16; x++)
                    {
                        float dx = x - 7.5f, dy = y - 7.5f;
                        fTex.SetPixel(x, y, dx * dx + dy * dy <= 49 ?
                            new Color(0.42f, 0.26f, 0.15f) : Color.clear);
                    }
                fTex.Apply();
                friendSR.sprite = Sprite.Create(fTex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
            }

            float fx = UnityEngine.Random.Range(3f, GardenManager.Instance.GardenWidth - 3f);
            float fy = UnityEngine.Random.Range(3f, GardenManager.Instance.GardenHeight - 3f);
            friendGO.transform.position = new Vector3(fx, fy, 0f);
            friendGO.transform.localScale = Vector3.one * 0.7f;

            var friendCol = friendGO.AddComponent<CircleCollider2D>();
            friendCol.isTrigger = true;
            friendCol.radius = 0.5f;

            // Rigidbody2D needed for trigger detection
            var friendRB = friendGO.AddComponent<Rigidbody2D>();
            friendRB.isKinematic = true;
            friendRB.gravityScale = 0f;

            friendGO.AddComponent<FriendHedgehog>();
        }

        // Night Overlay — subtle blue dusk, not too dark
        var nightGO = new GameObject("NightOverlay");
        var nightSR = nightGO.AddComponent<SpriteRenderer>();
        var nightTex = new Texture2D(1, 1);
        nightTex.SetPixel(0, 0, new Color(0.05f, 0.05f, 0.2f, 0.35f)); // Subtle dusk tint
        nightTex.Apply();
        nightSR.sprite = Sprite.Create(nightTex, new Rect(0,0,1,1), new Vector2(0.5f,0.5f), 1f);
        float mapW = GardenManager.Instance != null ? GardenManager.Instance.GardenWidth : 20f;
        float mapH = GardenManager.Instance != null ? GardenManager.Instance.GardenHeight : 20f;
        nightGO.transform.localScale = new Vector3(mapW + 10f, mapH + 10f, 1f);
        nightGO.transform.position = new Vector3(mapW / 2f, mapH / 2f, 0f);
        nightSR.sortingOrder = 5; // Just above background (-1000), below everything else

        Debug.Log("[PhaseController] Hedgehog spawned, night has fallen.");
    }

    private void SpawnFox()
    {
        var foxGO = new GameObject("Fox");
        if (GardenManager.Instance != null)
        {
            foxGO.transform.position = new Vector3(2f, GardenManager.Instance.GardenHeight - 2f, 0f);
        }
        else
        {
            foxGO.transform.position = new Vector3(2f, 10f, 0f);
        }

        var sr = foxGO.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 51;

        var foxSprite = Resources.Load<Sprite>("EgelGame/fox");
        if (foxSprite != null)
        {
            sr.sprite = foxSprite;
        }
        else
        {
            // Fallback: simple orange box
            var tex = new Texture2D(16, 16);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    tex.SetPixel(x, y, new Color(0.9f, 0.43f, 0.15f));
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0,0,16,16), new Vector2(0.5f,0.5f), 16f);
        }

        foxGO.AddComponent<FoxAI>();
    }

    private void SpawnMower()
    {
        if (GardenManager.Instance == null) return;

        var mowerGO = new GameObject("RobotMower");
        mowerGO.transform.position = new Vector3(
            GardenManager.Instance.GardenWidth - 2f,
            2f, 0f);
            
        mowerGO.AddComponent<RobotMowerAI>();
    }

    public void StartRain()
    {
        if (IsRaining || CurrentPhase != GamePhase.HedgehogVisit) return;
        IsRaining = true;

        Debug.Log("[PhaseController] It started raining! Puddles forming on paved tiles.");

        // Start lightning flash and dim the night overlay
        var nightGO = GameObject.Find("NightOverlay");
        if (nightGO != null)
        {
            var sr = nightGO.GetComponent<SpriteRenderer>();
            if (sr != null) StartCoroutine(FlashLightning(sr));
        }

        // Spawn Puddles on ALL Paved tiles
        if (GardenManager.Instance != null)
        {
            foreach (var obj in GardenManager.Instance.PlacedObjects)
            {
                if (obj.Type == GardenObject.ObjectType.Paved)
                {
                    SpawnPuddle(obj.transform.position);
                }
            }
        }
    }

    private void SpawnPuddle(Vector3 pos)
    {
        var puddleGO = new GameObject("RainPuddle");
        puddleGO.transform.position = pos;
        
        var sr = puddleGO.AddComponent<SpriteRenderer>();
        var tex = new Texture2D(16, 16);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                float dx = (x - 7.5f) / 7f;
                float dy = (y - 7.5f) / 5f; // Oval shape
                if (dx * dx + dy * dy <= 1f)
                    tex.SetPixel(x, y, new Color(0.2f, 0.4f, 0.8f, 0.6f)); // Semi-transparent blue
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        }
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0,0,16,16), new Vector2(0.5f,0.5f), 16f);
        sr.sortingOrder = 1; // Just above the paved tile

        var col = puddleGO.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 0.8f);

        // We don't need a custom script, HedgehogAI will just look for the "RainPuddle" name
    }

    private System.Collections.IEnumerator FlashLightning(SpriteRenderer sr)
    {
        // First strike
        sr.color = new Color(1f, 1f, 1f, 0.8f);
        yield return new WaitForSeconds(0.1f);
        sr.color = new Color(0.1f, 0.1f, 0.3f, 0.5f);
        yield return new WaitForSeconds(0.05f);
        // Second strike
        sr.color = new Color(0.9f, 0.9f, 1f, 0.6f);
        yield return new WaitForSeconds(0.1f);
        // Settle into rain tint
        sr.color = new Color(0.1f, 0.1f, 0.3f, 0.5f);
    }

    private void CalculateFinalScore()
    {
        // Set biodiversity score from tracker
        if (BiodiversityTracker.Instance != null && ScoreManager.Instance != null)
            ScoreManager.Instance.SetBiodiversity(BiodiversityTracker.Instance.BiodiversityScore);

        if (ScoreManager.Instance != null && GameManager.Instance != null)
            GameManager.Instance.TotalScore = ScoreManager.Instance.TotalScore;

        Debug.Log($"[PhaseController] Result phase. Total score: {GameManager.Instance?.TotalScore}");
    }

    // ── Transition helpers ──────────────────────────────────────

    /// <summary>
    /// Ends the hedgehog phase early if the player dies/starves.
    /// </summary>
    public void EndGameEarly(string reason)
    {
        if (CurrentPhase != GamePhase.HedgehogVisit) return;

        Debug.Log($"[PhaseController] Game Over triggered: {reason}");
        GameOverReason = reason;
        
        // Halve the score as a penalty
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddFood(-ScoreManager.Instance.FoodScore * 0.5f);
            ScoreManager.Instance.AddWater(-ScoreManager.Instance.WaterScore * 0.5f);
            ScoreManager.Instance.AddShelter(-ScoreManager.Instance.ShelterScore * 0.5f);
            ScoreManager.Instance.AddBiodiversity(-ScoreManager.Instance.BiodiversityScore * 0.5f);
        }

        StartPhase(GamePhase.Result);
    }

    /// <summary>
    /// Convenience method: transition from current phase to the next in sequence.
    /// </summary>
    public void AdvanceToNextPhase()
    {
        switch (CurrentPhase)
        {
            case GamePhase.Intro:          StartPhase(GamePhase.GardenBuild);   break;
            case GamePhase.GardenBuild:    StartPhase(GamePhase.HedgehogVisit); break;
            case GamePhase.HedgehogVisit:  StartPhase(GamePhase.Result);        break;
            case GamePhase.Result:         ManualRestart();                     break;
        }
    }

    private void ManualRestart()
    {
        Debug.Log("[PhaseController] Doing a manual replay reset to avoid SceneManager errors.");
            
        // Clean up insects and snacks
        var insects = FindObjectsByType<Insect>(FindObjectsSortMode.None);
        foreach (var inc in insects) Destroy(inc.gameObject);
            
        var snacks = FindObjectsByType<Collectible>(FindObjectsSortMode.None);
        foreach (var snack in snacks) Destroy(snack.gameObject);

        // Remove the existing hedgehog and fox manually just to be safe
        var hedgehog = FindAnyObjectByType<HedgehogAI>();
        if (hedgehog != null) Destroy(hedgehog.gameObject);
        
        var fox = FindAnyObjectByType<FoxAI>();
        if (fox != null) Destroy(fox.gameObject);
        
        var gardener = FindAnyObjectByType<PlayerController>();
        if (gardener != null) Destroy(gardener.gameObject);
            
        // Go back to intro
        StartPhase(GamePhase.Intro);
            
        // Re-enable the replay button for future clicks
        var resultUI = FindAnyObjectByType<ResultScreenUI>();
        if (resultUI != null)
        {
            var btns = resultUI.GetComponentsInChildren<UnityEngine.UI.Button>();
            foreach (var btn in btns) btn.interactable = true;
        }
    }

    // ── Fade coroutine ──────────────────────────────────────────

    /// <summary>
    /// Fades to black, executes <paramref name="onMidpoint"/>, then fades back.
    /// Uses unscaled time so pausing won't freeze the transition.
    /// </summary>
    public IEnumerator FadeTransition(Action onMidpoint)
    {
        if (fadeOverlay == null)
        {
            // No overlay assigned — execute immediately
            onMidpoint?.Invoke();
            IsTransitioning = false;
            yield break;
        }

        fadeOverlay.blocksRaycasts = true;

        // Fade to black
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeOverlay.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        fadeOverlay.alpha = 1f;

        // Mid-point callback
        onMidpoint?.Invoke();

        // One-frame pause so new UI can settle
        yield return null;

        // Fade back to transparent
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeOverlay.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        fadeOverlay.alpha = 0f;
        fadeOverlay.blocksRaycasts = false;
        
        IsTransitioning = false;
    }
}
