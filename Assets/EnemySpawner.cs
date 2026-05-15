using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [Header("--- Enemy Settings ---")]
    [Tooltip("ลาก Prefab ของศัตรูมาใส่ที่ช่องนี้")]
    public GameObject enemyPrefab;

    [Tooltip("ใส่จุดต่าง ๆ บนแผนที่ที่ต้องการให้ศัตรูไปเกิด")]
    public Transform[] spawnPoints;

    private void Awake()
    {
        // ทำเป็น Singleton เพื่อให้ QuestManager เรียกใช้ได้ง่ายๆ
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ฟังก์ชันสำหรับสั่งให้ศัตรูเกิด
    public void SpawnEnemies(int amount)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("🚨 ยังไม่ได้ใส่ Enemy Prefab ใน Inspector!");
            return;
        }

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("⚠️ ไม่มีจุด Spawn Points ศัตรูจะเกิดที่ตำแหน่งของ Spawner แทน");
        }

        for (int i = 0; i < amount; i++)
        {
            // สุ่มจุดเกิดจากรายการ spawnPointsที่มี
            Vector3 spawnPosition = transform.position;
            if (spawnPoints.Length > 0)
            {
                int randomIndex = Random.Range(0, spawnPoints.Length);
                spawnPosition = spawnPoints[randomIndex].position;
            }

            // คำสั่งสร้าง (Spawn) ศัตรูออกมาในเกม
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }

        Debug.Log($"<color=red>SPAWN:</color> สร้างศัตรูออกมาทั้งหมด {amount} ตัวเรียบร้อย!");
    }
}