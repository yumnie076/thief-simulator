using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Phase 3 — Hedgehog visit UI for Egel op Expeditie.
/// Shows a countdown timer, hunger bar, and safety indicator.
/// </summary>
public class HedgehogPhaseUI : MonoBehaviour
{
    // ── Inspector refs ──────────────────────────────────────────
    [Header("Text")]
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text timerText;

    [Header("Hunger Bar")]
    [Tooltip("Foreground fill image (Image Type = Filled).")]
    [SerializeField] private Image hungerBarFill;

    [Header("Safety Indicator")]
    [SerializeField] private TMP_Text safetyText;
    [SerializeField] private Image safetyIcon;

    // ── Timer duration (const so scene-serialized values can never override it) ──
    private const float totalTime = 30f;

    // ── Colours for hunger gradient ─────────────────────────────
    private static readonly Color HungerFull  = new Color32(90, 184, 74, 255);  // green #5ab84a
    private static readonly Color HungerEmpty = new Color32(217, 74, 61, 255);  // red   #d94a3d

    // ── State ───────────────────────────────────────────────────
    private float timeRemaining;
    private bool isRunning;

    // ── Lifecycle ───────────────────────────────────────────────
    private void Start()
    {
        if (headerText != null)
            headerText.text = "De egel zoekt voedsel en schuilplek...";

        if (PhaseController.Instance != null)
        {
            PhaseController.Instance.OnPhaseChanged += OnPhaseChanged;
            OnPhaseChanged(PhaseController.Instance.CurrentPhase);
        }
    }

    private void OnDestroy()
    {
        if (PhaseController.Instance != null)
            PhaseController.Instance.OnPhaseChanged -= OnPhaseChanged;
    }

    private void Update()
    {
        if (!isRunning) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isRunning = false;
            OnTimerExpired();
        }

        UpdateTimerDisplay();
    }

    // ── Timer ───────────────────────────────────────────────────
    private void UpdateTimerDisplay()
    {
        int seconds = Mathf.CeilToInt(timeRemaining);
        float score = ScoreManager.Instance != null ? ScoreManager.Instance.TotalScore : 0f;
        if (timerText != null) timerText.text = $"{seconds}s\nScore: {score:F0}";
    }

    private void OnTimerExpired()
    {
        Debug.Log("[HedgehogPhaseUI] Timer expired — transitioning to Result.");
        PhaseController.Instance?.StartPhase(PhaseController.GamePhase.Result);
    }

    // ── Public setters (called by HedgehogController) ───────────

    /// <summary>
    /// Update the hunger bar. Value 0 = starving, 1 = full.
    /// </summary>
    public void SetHunger(float normalized01)
    {
        normalized01 = Mathf.Clamp01(normalized01);
        if (hungerBarFill != null)
        {
            hungerBarFill.fillAmount = normalized01;
            hungerBarFill.color = Color.Lerp(HungerEmpty, HungerFull, normalized01);
        }
    }

    /// <summary>
    /// Update the safety indicator.
    /// </summary>
    public void SetSafety(bool isSafe)
    {
        if (safetyText != null)
            safetyText.text = isSafe ? "Veilig" : "Onveilig";

        if (safetyIcon != null)
            safetyIcon.color = isSafe ? HungerFull : HungerEmpty;
    }

    // ── Show / Hide ─────────────────────────────────────────────
    public void Show()
    {
        gameObject.SetActive(true);
        timeRemaining = totalTime;
        isRunning = true;

        // Force-set all labels at runtime to override scene-serialized emoji text
        if (headerText != null) headerText.text = "De egel zoekt voedsel en schuilplek...";
        if (safetyText != null) safetyText.text = "Veiligheid";

        // Find and fix the HungerLabel if it exists (set by Bootstrapper, could have old emoji)
        var hungerLabel = transform.Find("HungerLabel");
        if (hungerLabel != null)
        {
            var tmp = hungerLabel.GetComponent<TMP_Text>();
            if (tmp != null) tmp.text = "Honger";
        }

        SetHunger(1f);
        SetSafety(false);
        UpdateTimerDisplay();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        isRunning = false;
    }

    // ── Phase listener ──────────────────────────────────────────
    private void OnPhaseChanged(PhaseController.GamePhase phase)
    {
        if (phase == PhaseController.GamePhase.HedgehogVisit)
            Show();
        else
            Hide();
    }
}
