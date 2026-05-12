using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleporter : MonoBehaviour
{
    public string targetSceneName;

    private void OnTriggerEnter(Collider other)
    {
        // จุดเช็คที่ 1: มีอะไรมาชนหรือยัง?
        Debug.Log("มีการชนเกิดขึ้นกับ: " + other.name);

        if (other.CompareTag("Player"))
        {
            // จุดเช็คที่ 2: เป็น Player จริงไหม?
            Debug.Log("เช็ค Tag ผ่าน: นี่คือ Player");

            // จุดเช็คที่ 3: สถานะเควสใน QuestManager เป็นอะไร?
            Debug.Log("สถานะเควสปัจจุบันคือ: " + QuestManager.Instance.currentQuestState);

            if (QuestManager.Instance.currentQuestState == QuestManager.QuestState.Completed)
            {
                Debug.Log("เงื่อนไขครบ! กำลังย้ายไปซีน: " + targetSceneName);
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.Log("<color=red>ย้ายไม่ได้:</color> เพราะสถานะเควสยังไม่เป็น Completed");
            }
        }
    }
}