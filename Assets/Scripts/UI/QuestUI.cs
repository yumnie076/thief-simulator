using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI overlay for the quest system.
/// </summary>
public class QuestUI : MonoBehaviour
{
    private TextMeshProUGUI questText;

    private void Start()
    {
        // Wait one frame to ensure QuestManager is initialized
        Invoke(nameof(Init), 0.1f);
    }

    private void Init()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated += UpdateText;
            QuestManager.Instance.OnAllQuestsCompleted += HandleCompleted;
            UpdateText();
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated -= UpdateText;
            QuestManager.Instance.OnAllQuestsCompleted -= HandleCompleted;
        }
    }

    public void AssignText(TextMeshProUGUI tmp)
    {
        questText = tmp;
    }

    private void UpdateText()
    {
        if (questText == null || QuestManager.Instance == null) return;
        
        var q = QuestManager.Instance;
        questText.text = "<b>Huidige Missies:</b>\n" +
                         $"- Plant bloemen ({q.FlowersPlanted}/{q.TargetFlowers})\n" +
                         $"- Lok insecten ({q.InsectsSpawned}/{q.TargetInsects})\n" +
                         $"- Voer egel ({q.BugsEaten}/{q.TargetBugsEaten})";
    }

    private void HandleCompleted()
    {
        if (questText != null)
        {
            questText.text = "<b>Missies Voltooid!</b>\nDe tuin leeft!\n\nJe kunt Phase afsluiten.";
            questText.color = new Color(0.2f, 0.8f, 0.2f); // Green
        }
    }
}
