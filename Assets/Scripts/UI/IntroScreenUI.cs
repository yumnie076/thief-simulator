using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Phase 1 — Intro question screen for Egel op Expeditie.
/// Asks the player how green their garden currently is.
/// </summary>
public class IntroScreenUI : MonoBehaviour
{

    [Header("Text Fields")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text footerText;

    [Header("Buttons (top to bottom: hard → easy)")]
    [SerializeField] private Button hardButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button easyButton;

    // ── Colour constants ────────────────────────────────────────
    private static readonly Color ColourRed    = new Color32(217, 74, 61, 255);   // #d94a3d
    private static readonly Color ColourYellow = new Color32(232, 196, 71, 255);  // #e8c447
    private static readonly Color ColourGreen  = new Color32(90, 184, 74, 255);   // #5ab84a

    private const float HoverScale = 1.05f;

    // ── Lifecycle ───────────────────────────────────────────────
    private void Start()
    {
        SetupTexts();
        SetupButtons();

        // Listen to phase changes so we auto-show / hide
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
        // Breathing animation on the title
        if (titleText != null && gameObject.activeSelf)
        {
            float breathe = 1f + Mathf.Sin(Time.time * 1.5f) * 0.02f;
            titleText.transform.localScale = new Vector3(breathe, breathe, 1f);
        }
    }

    // ── Text setup ──────────────────────────────────────────────
    private void SetupTexts()
    {
        if (titleText != null)    titleText.text    = "EGEL OP EXPEDITIE";
        if (subtitleText != null) subtitleText.text  = "Hoe egelvriendelijk is jouw tuin?";
        if (questionText != null) questionText.text  = "Hoe groen is jouw tuin het meest?";
        if (footerText != null)   footerText.text    = "Een Tuinen van de Toekomst x Avans project";
    }

    // ── Button setup ────────────────────────────────────────────
    private void SetupButtons()
    {
        ConfigureButton(hardButton,   "Level 3: Expert\nEen versteende tuin vol afval. Maak het leefbaar!",       ColourRed,    0);
        ConfigureButton(mediumButton, "Level 1: Beginner\nBouw een simpele tuin met water en schuilplek.",      ColourYellow, 1);
        ConfigureButton(easyButton,   "Level 2: Gevorderd\nEen groene tuin, maar kan het nóg beter?",           ColourGreen,  2);
    }

    private void ConfigureButton(Button btn, string label, Color colour, int startState)
    {
        if (btn == null) return;

        // Set button label
        TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();
        if (txt != null) txt.text = label;

        // Tint the button image
        Image img = btn.GetComponent<Image>();
        if (img != null) img.color = colour;

        // Click handler
        btn.onClick.AddListener(() => OnDifficultySelected(startState));

        // Hover scale effect via EventTrigger
        AddHoverEffect(btn.gameObject);
    }

    private void OnDifficultySelected(int startState)
    {
        if (PhaseController.Instance == null) return;

        // Disable buttons to prevent double-fire
        SetButtonsInteractable(false);

        PhaseController.Instance.SetGardenStartState(startState);
        PhaseController.Instance.StartPhase(PhaseController.GamePhase.GardenBuild);
    }

    // ── Hover effect ────────────────────────────────────────────
    private void AddHoverEffect(GameObject target)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null) trigger = target.AddComponent<EventTrigger>();

        // Pointer Enter → scale up
        EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((_) => target.transform.localScale = Vector3.one * HoverScale);
        trigger.triggers.Add(enterEntry);

        // Pointer Exit → scale back
        EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((_) => target.transform.localScale = Vector3.one);
        trigger.triggers.Add(exitEntry);
    }

    // ── Show / Hide ─────────────────────────────────────────────
    public void Show()
    {
        gameObject.SetActive(true);
        SetButtonsInteractable(true);

        // Force-set all labels at runtime to override scene-serialized text
        SetupTexts();
        SetupButtons();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (hardButton != null)   hardButton.interactable   = interactable;
        if (mediumButton != null) mediumButton.interactable = interactable;
        if (easyButton != null)   easyButton.interactable   = interactable;
    }

    // ── Phase listener ──────────────────────────────────────────
    private void OnPhaseChanged(PhaseController.GamePhase phase)
    {
        if (phase == PhaseController.GamePhase.Intro)
            Show();
        else
            Hide();
    }
}
