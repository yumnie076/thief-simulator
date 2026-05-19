using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Paused, Win, Lose }
    public GameState State { get; private set; } = GameState.Playing;

    private void Awake()
    {
        // Singleton guard
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && State == GameState.Playing)
            TogglePause();

        if (Input.GetKeyDown(KeyCode.R) && (State == GameState.Win || State == GameState.Lose))
            Restart();
    }

    public void TogglePause()
    {
        if (State == GameState.Paused)
        {
            State = GameState.Playing;
            Time.timeScale = 1f;
        }
        else
        {
            State = GameState.Paused;
            Time.timeScale = 0f;
        }
        UIManager.Instance?.RefreshPauseState(State == GameState.Paused);
    }

    public void TriggerWin()
    {
        if (State != GameState.Playing) return;
        State = GameState.Win;
        Time.timeScale = 0f;
        AudioManager.Instance?.PlayPickup();
        UIManager.Instance?.ShowWin(ScoreManager.Instance?.TotalScore ?? 0);
    }

    public void TriggerLose()
    {
        if (State != GameState.Playing) return;
        State = GameState.Lose;
        Time.timeScale = 0f;
        AudioManager.Instance?.PlayAlarm();
        UIManager.Instance?.ShowLose();
        Camera.main?.GetComponent<CameraFollow>()?.Shake(0.5f, 0.5f);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
