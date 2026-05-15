using UnityEngine;
using UnityEngine.SceneManagement; // จำเป็นต้องใส่เพื่อใช้ระบบเปลี่ยนซีน

public class SceneChanger : MonoBehaviour
{
    [Header("ตั้งค่าชื่อซีนที่ต้องการข้ามไป")]
    [SerializeField] private string targetSceneName;

    // ฟังก์ชันนี้จะทำงานเมื่อมีวัตถุที่มี Collider เดินเข้ามาชน
    private void OnTriggerEnter(Collider other)
    {
        // ตรวจสอบว่าวัตถุที่เดินมาเหยียบมี Tag ว่า "Player" ใช่หรือไม่
        if (other.CompareTag("Player"))
        {
            // ทำการโหลดซีนใหม่ตามชื่อที่เราตั้งไว้
            SceneManager.LoadScene(targetSceneName);
        }
    }
}