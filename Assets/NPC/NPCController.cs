using UnityEngine;
using System.Collections;
using TMPro;

public class NPCController : MonoBehaviour
{
    [Header("--- Scene Transition (เฉพาะ NPC B) ---")]
    [Tooltip("ติ๊กถูกถ้าต้องการให้เปลี่ยนฉากหลังส่งเควสสำเร็จ")]
    public bool loadSceneAfterQuest = false;
    [Tooltip("ชื่อ Scene ที่จะให้โหลดไป (เช่น VideoCutscene)")]
    public string nextSceneToLoad = "";



    public enum NPCRole { NPC_A_Giver, NPC_B_Receiver }

    [Header("--- NPC Identity ---")]
    public string npcName = "ชื่อ NPC";
    public NPCRole npcRole;
    public float rotationSpeed = 5f;

    [Header("--- Dialogue Settings ---")]
    [TextArea(3, 10)] public string[] mainQuestDialogue;
    [TextArea(3, 10)] public string[] afterDialogue;
    [TextArea(3, 10)] public string[] notEnoughItemsDialogue;

    [Header("--- HUD Text (For NPC A) ---")]
    public string questHUDOngoing = "ตามหาไอเทม";
    public string questHUDDone = "เก็บครบแล้ว! ไปส่งเควสกัน";

    [Header("--- UI & Audio ---")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public GameObject interactPrompt;
    public AudioSource audioSource;
    public AudioClip typeSound;
    public AudioClip talkStartSound;

    private Transform playerTransform;
    private bool isTalking;
    private bool isTyping;
    private bool canInteract;
    private int currentLineIndex;
    private string[] currentActiveLines;

    void Update()
    {
        // เช็คการกดคุย
        if (canInteract && !isTalking && Input.GetKeyDown(KeyCode.E))
        {
            StartDialogue();
        }
        else if (isTalking && Input.GetKeyDown(KeyCode.E))
        {
            HandleDialogueProgression();
        }

        // ระบบหันหน้าเข้าหากัน (Auto Face-to-Face)
        if (isTalking && playerTransform != null)
        {
            FaceTarget(playerTransform, transform); // NPC มอง Player
            FaceTarget(transform, playerTransform); // Player มอง NPC
        }
    }

    void StartDialogue()
    {
        DetermineDialogue();
        isTalking = true;
        currentLineIndex = 0;

        if (interactPrompt) interactPrompt.SetActive(false);
        if (dialoguePanel) dialoguePanel.SetActive(true);
        if (nameText) nameText.text = npcName;
        if (audioSource && talkStartSound) audioSource.PlayOneShot(talkStartSound);

        TogglePlayerMovement(false); // Freeze ผู้เล่น
        StartCoroutine(TypeSentence(currentActiveLines[currentLineIndex]));
    }

    void HandleDialogueProgression()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentActiveLines[currentLineIndex];
            isTyping = false;
        }
        else
        {
            currentLineIndex++;
            if (currentLineIndex < currentActiveLines.Length)
                StartCoroutine(TypeSentence(currentActiveLines[currentLineIndex]));
            else
                EndDialogue();
        }
    }

    void EndDialogue()
    {
        isTalking = false;
        if (dialoguePanel) dialoguePanel.SetActive(false);
        TogglePlayerMovement(true); // ปลดล็อกผู้เล่น

        // สั่งงาน QuestManager หลังคุยจบ
        QuestManager.QuestState state = QuestManager.Instance.currentQuestState;

        if (npcRole == NPCRole.NPC_A_Giver && state == QuestManager.QuestState.NotStarted)
        {
            QuestManager.Instance.currentQuestState = QuestManager.QuestState.InProgress;
            QuestManager.Instance.ShowQuestHUD(questHUDOngoing, questHUDDone, true);
        }
        else if (npcRole == NPCRole.NPC_B_Receiver && state == QuestManager.QuestState.InProgress)
        {
            if (QuestManager.Instance.currentItems >= QuestManager.Instance.targetItems)
            {
                QuestManager.Instance.FinishQuest();

                // 👇👇 [อัปเดตใหม่] เช็คก่อนว่า NPC ตัวนี้ถูกตั้งค่าให้เปลี่ยนฉากไหม 👇👇
                if (loadSceneAfterQuest && !string.IsNullOrEmpty(nextSceneToLoad))
                {
                    Debug.Log($"ส่งเควสสำเร็จ! NPC ตัวนี้สั่งให้ตัดเข้าซีน: {nextSceneToLoad}");
                    UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneToLoad);
                }
            }
        }
    }

    // ฟังก์ชันช่วยจัดการบทสนทนา
    void DetermineDialogue()
    {
        QuestManager.QuestState state = QuestManager.Instance.currentQuestState;
        if (npcRole == NPCRole.NPC_A_Giver)
        {
            currentActiveLines = (state == QuestManager.QuestState.NotStarted) ? mainQuestDialogue : afterDialogue;
        }
        else // NPC_B
        {
            if (state == QuestManager.QuestState.InProgress)
                currentActiveLines = (QuestManager.Instance.currentItems >= QuestManager.Instance.targetItems) ? mainQuestDialogue : notEnoughItemsDialogue;
            else
                currentActiveLines = afterDialogue;
        }
    }

    // ฟังก์ชันการหันหน้า
    void FaceTarget(Transform target, Transform self)
    {
        Vector3 dir = (target.position - self.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            self.rotation = Quaternion.Slerp(self.rotation, lookRot, Time.deltaTime * rotationSpeed);
        }
    }

    // ฟังก์ชัน Freeze ผู้เล่น (ต้องมีสคริปต์เดินที่ชื่อ PlayerMovement หรือเปลี่ยนชื่อให้ตรงกัน)
    void TogglePlayerMovement(bool enable)
    {
        if (playerTransform == null) return;
        // ปรับแก้ชื่อสคริปต์ควบคุมการเดินของคุณตรงนี้
        MonoBehaviour moveScript = playerTransform.GetComponent<MonoBehaviour>();
        if (moveScript != null) moveScript.enabled = enable;
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            if (audioSource && typeSound) audioSource.PlayOneShot(typeSound);
            yield return new WaitForSeconds(0.05f);
        }
        isTyping = false;
    }

    // ระบบ Trigger Detection
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = true;
            playerTransform = other.transform;
            if (interactPrompt) interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            if (interactPrompt) interactPrompt.SetActive(false);
        }
    }
}