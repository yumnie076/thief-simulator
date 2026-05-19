using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Phase 4 — Full-screen result overlay for Egel op Expeditie.
/// Shows score breakdown, outcome title, random tips, and replay/quit buttons.
/// </summary>
public class ResultScreenUI : MonoBehaviour
{
    // ── Inspector refs ──────────────────────────────────────────
    [Header("Title & Message")]
    [SerializeField] private TMP_Text titleText;

    [Header("Score Breakdown")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Tips")]
    [SerializeField] private TMP_Text tipsText;

    [Header("Buttons")]
    [SerializeField] private Button replayButton;
    [SerializeField] private Button quitButton;

    // ── Score thresholds ────────────────────────────────────────
    private const float ThresholdGreat   = 80f;
    private const float ThresholdPartial = 40f;

    // ── Lifecycle ───────────────────────────────────────────────
    private void Start()
    {
        SetupButtons();

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

    // ── Button setup ────────────────────────────────────────────
    private void SetupButtons()
    {
        if (replayButton != null) replayButton.onClick.AddListener(OnReplayClicked);
        if (quitButton != null)   quitButton.onClick.AddListener(OnQuitClicked);
    }

    // ── Public API ──────────────────────────────────────────────

    /// <summary>
    /// Populate the result screen with scores and display it.
    /// </summary>
    public void DisplayResults(float biodiversity, float food, float water, float shelter, float total)
    {
        // ─ Title based on outcome ─
        if (titleText != null)
        {
            if (total >= ThresholdGreat)
                titleText.text = "🎉 Jouw tuin is een egel-paradijs!";
            else if (total >= ThresholdPartial)
                titleText.text = "👍 Een goed begin voor de egel!";
            else
                titleText.text = "😔 De egel had het moeilijk in jouw tuin...";
        }

        // ─ Score breakdown (single text field) ─
        if (scoreText != null)
        {
            scoreText.text = $"EGEL SCORE\n\n" +
                $"Biodiversiteit:      {biodiversity:F0} punten\n" +
                $"Voedsel gevonden:    {food:F0} punten\n" +
                $"Water bereikt:       {water:F0} punten\n" +
                $"Veilige schuilplek:  {shelter:F0} punten\n" +
                $"───────────────────\n" +
                $"<b>TOTAAL:             {total:F0} punten</b>";
        }

        // ─ Random tips (pick 3) ─
        if (tipsText != null)
        {
            List<string> allTips = EducationContent.EndTips.Values.ToList();
            ShuffleList(allTips);
            int count = Mathf.Min(3, allTips.Count);
            tipsText.text = string.Join("\n\n", allTips.GetRange(0, count));
        }

        Show();
    }

    // ── Helpers ─────────────────────────────────────────────────

    /// <summary>Fisher-Yates shuffle.</summary>
    private static void ShuffleList<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ── Button handlers ─────────────────────────────────────────
    private void OnReplayClicked()
    {
        if (replayButton != null) replayButton.interactable = false; // prevent double-fire
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Show / Hide ─────────────────────────────────────────────
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // ── Phase listener ──────────────────────────────────────────
    private void OnPhaseChanged(PhaseController.GamePhase phase)
    {
        if (phase == PhaseController.GamePhase.Result)
        {
            // Auto-populate from ScoreManager if available
            if (ScoreManager.Instance != null)
            {
                DisplayResults(
                    ScoreManager.Instance.BiodiversityScore,
                    ScoreManager.Instance.FoodScore,
                    ScoreManager.Instance.WaterScore,
                    ScoreManager.Instance.ShelterScore,
                    ScoreManager.Instance.TotalScore
                );
            }
            else
            {
                Show();
            }
        }
        else
        {
            Hide();
        }
    }
}
