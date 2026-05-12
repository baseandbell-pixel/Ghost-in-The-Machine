using UnityEngine;

public class ItemCollectible : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // เก็บได้เฉพาะตอนเควสกำลังดำเนินอยู่ (InProgress)
        if (other.CompareTag("Player") && QuestManager.Instance.currentQuestState == QuestManager.QuestState.InProgress)
        {
            QuestManager.Instance.AddItem();
            Destroy(gameObject); // ทำลายไอเทมเมื่อเก็บแล้ว
        }
    }
}