using UnityEngine;

/// <summary>
/// Coordinates all UI panels for the 4-phase game loop.
/// Each phase has its own UI panel that this manager shows/hides.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Phase Panels")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private GameObject buildPanel;
    [SerializeField] private GameObject hedgehogPanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private GameObject fadePanel;

    [Header("References")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

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
        if (PhaseController.Instance != null)
        {
            PhaseController.Instance.OnPhaseChanged += OnPhaseChanged;
            // Initialize first state
            OnPhaseChanged(PhaseController.Instance.CurrentPhase);
        }
    }

    private void OnDestroy()
    {
        if (PhaseController.Instance != null)
        {
            PhaseController.Instance.OnPhaseChanged -= OnPhaseChanged;
        }
    }

    private void OnPhaseChanged(PhaseController.GamePhase phase)
    {
        HideAll();
        switch (phase)
        {
            case PhaseController.GamePhase.Intro:
                if (introPanel != null) introPanel.SetActive(true);
                break;
            case PhaseController.GamePhase.GardenBuild:
                if (buildPanel != null) buildPanel.SetActive(true);
                break;
            case PhaseController.GamePhase.HedgehogVisit:
                if (hedgehogPanel != null) hedgehogPanel.SetActive(true);
                break;
            case PhaseController.GamePhase.Result:
                if (resultPanel != null) resultPanel.SetActive(true);
                break;
        }
    }

    public void HideAll()
    {
        if (introPanel != null) introPanel.SetActive(false);
        if (buildPanel != null) buildPanel.SetActive(false);
        if (hedgehogPanel != null) hedgehogPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    /// <summary>Get the fade CanvasGroup for transitions.</summary>
    public CanvasGroup GetFadeGroup()
    {
        return fadeCanvasGroup;
    }
}
