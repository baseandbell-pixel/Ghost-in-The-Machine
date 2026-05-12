using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public enum QuestState { NotStarted, InProgress, Completed }
    [Header("--- Quest Status ---")]
    public QuestState currentQuestState = QuestState.NotStarted;
    public int currentItems = 0;
    public int targetItems = 5;

    [Header("--- UI References ---")]
    public GameObject questHUDPanel;
    public TextMeshProUGUI questStatusText;

    [HideInInspector] public string ongoingHUDText;
    [HideInInspector] public string completedHUDText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start() { if (questHUDPanel) questHUDPanel.SetActive(false); }

    public void AddItem()
    {
        currentItems++;
        UpdateQuestHUD();
    }

    public void UpdateQuestHUD()
    {
        if (questStatusText == null) return;

        if (currentItems >= targetItems)
            questStatusText.text = $"<color=yellow>{completedHUDText}</color>";
        else
            questStatusText.text = $"{ongoingHUDText} ({currentItems}/{targetItems})";
    }

    public void ShowQuestHUD(string ongoing, string done, bool show)
    {
        ongoingHUDText = ongoing;
        completedHUDText = done;
        if (questHUDPanel) questHUDPanel.SetActive(show);
        UpdateQuestHUD();
    }

    public void FinishQuest()
    {
        currentItems = 0;
        currentQuestState = QuestState.Completed;
        if (questHUDPanel) questHUDPanel.SetActive(false);
        Debug.Log("<color=green>SUCCESS:</color> Quest Finished!");
    }
}