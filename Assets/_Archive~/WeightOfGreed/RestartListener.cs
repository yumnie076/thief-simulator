using UnityEngine;

/// <summary>
/// Listens for R key on win/lose screens and triggers restart.
/// Attached to UICanvas by SceneBootstrapper.
/// </summary>
public class RestartListener : MonoBehaviour
{
    private void Update()
    {
        var state = GameManager.Instance?.State;
        if ((state == GameManager.GameState.Win || state == GameManager.GameState.Lose)
            && Input.GetKeyDown(KeyCode.R))
        {
            GameManager.Instance.Restart();
        }
    }
}
