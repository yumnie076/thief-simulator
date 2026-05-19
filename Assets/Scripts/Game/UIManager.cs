using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    public GameObject hudPanel;
    public TMP_Text scoreText;
    public TMP_Text weightText;
    public TMP_Text itemsText;
    public UnityEngine.UI.Image flashImage;
    public GameObject sneakTooltip;        // "Hold SHIFT to sneak"

    [Header("Win Panel")]
    public GameObject winPanel;
    public TMP_Text winScoreText;

    [Header("Lose Panel")]
    public GameObject losePanel;

    [Header("Pause Panel")]
    public GameObject pausePanel;

    private float _tooltipTimer = 10f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        winPanel?.SetActive(false);
        losePanel?.SetActive(false);
        pausePanel?.SetActive(false);
        sneakTooltip?.SetActive(true);
        RefreshHUD(0, 0, 0);
    }

    private void Update()
    {
        if (_tooltipTimer > 0)
        {
            _tooltipTimer -= Time.unscaledDeltaTime;
            if (_tooltipTimer <= 0) sneakTooltip?.SetActive(false);
        }
    }

    public void RefreshHUD(int score, int weight, int items)
    {
        if (scoreText)  scoreText.text  = $"Score: {score}";
        if (weightText)
        {
            string newText = $"Weight: {weight}";
            if (weightText.text != newText)
            {
                weightText.text = newText;
                if (gameObject.activeInHierarchy) StartCoroutine(PunchScale(weightText.transform));
            }
            // 2D warning territory
            weightText.color = (weight >= 8) ? Color.red : Color.white;
        }
        if (itemsText)  itemsText.text  = $"Items: {items}";
    }

    public void FlashPickup()
    {
        if (flashImage == null) return;
        StartCoroutine(FlashRoutine());
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        float t = 0;
        float duration = 0.2f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Sin((t / duration) * Mathf.PI) * 0.15f;
            flashImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
        flashImage.color = new Color(1, 1, 1, 0);
    }

    private System.Collections.IEnumerator PunchScale(Transform t)
    {
        Vector3 orig = Vector3.one;
        t.localScale = orig * 1.5f;
        float time = 0;
        while (time < 0.3f)
        {
            time += Time.unscaledDeltaTime;
            t.localScale = Vector3.Lerp(orig * 1.5f, orig, time / 0.3f);
            yield return null;
        }
        t.localScale = orig;
    }

    public void ShowWin(int finalScore)
    {
        winPanel?.SetActive(true);
        int stolen = ScoreManager.Instance != null ? ScoreManager.Instance.ItemCount : 0;
        int left = ScoreManager.Instance != null ? (ScoreManager.Instance.TotalItemsInLevel - stolen) : 0;
        int bonus = left * 10;
        int total = finalScore + bonus;

        if (winScoreText) 
        {
            winScoreText.text = $"Items stolen: {stolen}\n" +
                                $"Total value: {finalScore}\n" +
                                $"Items LEFT BEHIND: {left}\n\n" +
                                $"\"Less is more\" bonus: +{bonus}\n\n" +
                                $"FINAL SCORE: {total}";
        }
    }

    public void ShowLose()
    {
        losePanel?.SetActive(true);
    }

    public void RefreshPauseState(bool isPaused)
    {
        pausePanel?.SetActive(isPaused);
    }
}
