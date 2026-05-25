using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Educational fact popup shown at top-center of the screen.
/// Fades in, waits 3 seconds, fades out. New calls cancel any running popup.
/// </summary>
public class EducationPopup : MonoBehaviour
{
    public static EducationPopup Instance { get; private set; }

    // ── Inspector refs ──────────────────────────────────────────
    [Header("UI Elements")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image backgroundPanel;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration  = 0.3f;
    [SerializeField] private float displayDuration = 3.0f;
    [SerializeField] private float fadeOutDuration  = 0.3f;

    // ── Internal ────────────────────────────────────────────────
    private Coroutine activeRoutine;

    // ── Singleton ───────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Start invisible
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    // ── Public API ──────────────────────────────────────────────

    /// <summary>
    /// Show an educational fact by its key (must exist in EducationContent.Facts).
    /// Cancels any currently displayed popup.
    /// </summary>
    public void Show(string factKey)
    {
        if (!EducationContent.Facts.TryGetValue(factKey, out string fact))
        {
            Debug.LogWarning($"[EducationPopup] No fact found for key '{factKey}'");
            return;
        }

        ShowMessage(fact);
    }

    public void ShowMessage(string message)
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(ShowRoutine(message));
    }

    // ── Coroutine ───────────────────────────────────────────────
    private IEnumerator ShowRoutine(string text)
    {
        if (messageText != null) messageText.text = text;

        // Fade in
        yield return FadeAlpha(0f, 1f, fadeInDuration);

        // Hold
        yield return new WaitForSecondsRealtime(displayDuration);

        // Fade out
        yield return FadeAlpha(1f, 0f, fadeOutDuration);

        activeRoutine = null;
    }

    private IEnumerator FadeAlpha(float from, float to, float duration)
    {
        if (canvasGroup == null) yield break;

        canvasGroup.blocksRaycasts = (to > 0f);
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
