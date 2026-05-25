using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Phase 2 — Build-phase HUD for Egel op Expeditie.
/// Left sidebar with 7 tool buttons, top-right counters, bottom-right "done" button.
/// </summary>
public class BuildPhaseUI : MonoBehaviour
{
    // ── Inspector refs ──────────────────────────────────────────

    [Header("Tool Buttons (order: RemoveTile, Flower, Bush, Tree, Pond, LeafPile, House)")]
    [SerializeField] private Button[] toolButtons = new Button[7];

    [Header("Tool Button Highlight")]
    [Tooltip("Outline / border image on each tool button, toggled for selected tool.")]
    [SerializeField] private Image[] toolHighlights = new Image[7];

    [Header("HUD Texts")]
    [SerializeField] private TMP_Text actionsText;
    [SerializeField] private TMP_Text biodiversityText;

    [Header("Done Button")]
    [SerializeField] private Button doneButton;
    [SerializeField] private TMP_Text doneButtonText;

    // ── Constants ───────────────────────────────────────────────
    private static readonly Color AccentColour = new Color32(90, 184, 74, 255); // #5ab84a
    private static readonly Color InactiveColour = new Color(1f, 1f, 1f, 0f);   // transparent

    private static readonly string[] ToolLabels =
    {
        "[1]",
        "[2]",
        "[3]",
        "[4]",
        "[5]",
        "[6]",
        "[7]"
    };

    // Maps button index to the string key used by EducationContent & GardenManager
    private static readonly string[] ToolKeys =
    {
        "RemoveTile", "Flower", "Bush", "Tree", "Pond", "LeafPile", "House"
    };

    // ── State ───────────────────────────────────────────────────
    private int selectedToolIndex = -1;
    private int remainingActions = 0;

    // ── Lifecycle ───────────────────────────────────────────────
    private void Start()
    {
        SetupToolButtons();
        SetupDoneButton();

        if (PhaseController.Instance != null)
        {
            PhaseController.Instance.OnPhaseChanged += OnPhaseChanged;
            OnPhaseChanged(PhaseController.Instance.CurrentPhase);
        }
    }

    private void Update()
    {
        // Keyboard shortcuts (1-7) for selecting tools
        if (PhaseController.Instance == null || PhaseController.Instance.CurrentPhase != PhaseController.GamePhase.GardenBuild)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) SelectTool(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) SelectTool(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) SelectTool(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) SelectTool(3);
        else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) SelectTool(4);
        else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) SelectTool(5);
        else if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7)) SelectTool(6);
    }

    private void OnDestroy()
    {
        if (PhaseController.Instance != null)
            PhaseController.Instance.OnPhaseChanged -= OnPhaseChanged;

        // Unsubscribe from garden events if available
        UnsubscribeGardenEvents();
    }

    // ── Tool button setup ───────────────────────────────────────
    private void SetupToolButtons()
    {
        for (int i = 0; i < toolButtons.Length && i < ToolLabels.Length; i++)
        {
            if (toolButtons[i] == null) continue;

            // Set label
            TMP_Text label = toolButtons[i].GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = ToolLabels[i];

            // Click handler (captured by value)
            int index = i;
            toolButtons[i].onClick.AddListener(() => SelectTool(index));

            // Start with highlight off
            if (i < toolHighlights.Length && toolHighlights[i] != null)
                toolHighlights[i].color = InactiveColour;
        }
    }

    private void SetupDoneButton()
    {
        if (doneButtonText != null) doneButtonText.text = "Klaar — Roep de egel!";
        if (doneButton != null) doneButton.onClick.AddListener(OnDoneClicked);
    }

    // ── Tool selection ──────────────────────────────────────────

    /// <summary>Select a tool by index (0-6).</summary>
    public void SelectTool(int index)
    {
        selectedToolIndex = index;

        // Update highlights and button background fallback
        for (int i = 0; i < toolHighlights.Length; i++)
        {
            if (toolHighlights[i] != null)
            {
                toolHighlights[i].color = (i == index) ? AccentColour : InactiveColour;
            }
            else if (i < toolButtons.Length && toolButtons[i] != null)
            {
                var img = toolButtons[i].GetComponent<Image>();
                if (img != null)
                {
                    img.color = (i == index) ? AccentColour : new Color(0.95f, 0.95f, 0.95f, 1f);
                }

                // Change text color for premium readability
                var txt = toolButtons[i].GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    txt.color = (i == index) ? Color.white : new Color(0.2f, 0.2f, 0.2f, 1f);
                }
            }
        }

        // Bridge to GardenManager
        if (GardenManager.Instance != null && index >= 0 && index < 7)
        {
            PlaceableTool.ToolType[] types = {
                PlaceableTool.ToolType.RemoveTile,
                PlaceableTool.ToolType.Flower,
                PlaceableTool.ToolType.Bush,
                PlaceableTool.ToolType.Tree,
                PlaceableTool.ToolType.Pond,
                PlaceableTool.ToolType.LeafPile,
                PlaceableTool.ToolType.HedgehogHouse
            };
            GardenManager.Instance.SelectTool(types[index]);
        }
    }

    /// <summary>Returns the currently selected tool key (e.g. "Flower") or null.</summary>
    public string GetSelectedToolKey()
    {
        if (selectedToolIndex < 0 || selectedToolIndex >= ToolKeys.Length) return null;
        return ToolKeys[selectedToolIndex];
    }

    // ── HUD updates ─────────────────────────────────────────────

    /// <summary>Called when an action is used in GardenManager.</summary>
    public void UpdateActions(int remaining)
    {
        remainingActions = remaining;
        if (actionsText != null) actionsText.text = $"Acties: {remaining}";

        // Gray-out tools when no actions left
        bool canPlace = remaining > 0;
        foreach (Button btn in toolButtons)
        {
            if (btn != null) btn.interactable = canPlace;
        }
    }

    /// <summary>Called when biodiversity score changes.</summary>
    public void UpdateBiodiversity(float score)
    {
        if (biodiversityText != null)
            biodiversityText.text = $"Biodiversiteit: {score:F0}";
    }

    // ── Done button ─────────────────────────────────────────────
    private void OnDoneClicked()
    {
        if (doneButton != null) doneButton.interactable = false; // prevent double-fire
        PhaseController.Instance?.StartPhase(PhaseController.GamePhase.HedgehogVisit);
    }

    // ── Show / Hide ─────────────────────────────────────────────
    public void Show()
    {
        gameObject.SetActive(true);
        SubscribeGardenEvents();

        // Initialize HUD
        if (GardenManager.Instance != null)
            UpdateActions(GardenManager.Instance.actionsRemaining);

        if (BiodiversityTracker.Instance != null)
            UpdateBiodiversity(BiodiversityTracker.Instance.BiodiversityScore);

        // Highlight the default tool (Flower, index 1) immediately on show
        SelectTool(1);

        if (EducationPopup.Instance != null)
        {
            EducationPopup.Instance.ShowMessage("Maak de tuin egelvriendelijk!\nVergeet niet een egelhuisje te plaatsen om straks te schuilen voor de vos!");
        }
    }

    public void Hide()
    {
        UnsubscribeGardenEvents();
        gameObject.SetActive(false);
    }

    // ── Phase listener ──────────────────────────────────────────
    private void OnPhaseChanged(PhaseController.GamePhase phase)
    {
        if (phase == PhaseController.GamePhase.GardenBuild)
            Show();
        else
            Hide();
    }

    // ── Garden event wiring ─────────────────────────────────────
    private void SubscribeGardenEvents()
    {
        if (GardenManager.Instance != null)
            GardenManager.Instance.OnActionUsed += UpdateActions;
        if (BiodiversityTracker.Instance != null)
            BiodiversityTracker.Instance.OnScoreChanged += UpdateBiodiversity;
    }

    private void UnsubscribeGardenEvents()
    {
        if (GardenManager.Instance != null)
            GardenManager.Instance.OnActionUsed -= UpdateActions;
        if (BiodiversityTracker.Instance != null)
            BiodiversityTracker.Instance.OnScoreChanged -= UpdateBiodiversity;
    }
}
