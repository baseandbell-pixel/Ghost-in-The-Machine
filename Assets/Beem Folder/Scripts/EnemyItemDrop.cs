using UnityEngine;

public class EnemyItemDrop : MonoBehaviour
{
    [Header("=== Drop Item Settings ===")]
    [Tooltip("ลาก Prefab สิ่งของที่อยากให้ดรอปมาใส่ตรงนี้")]
    public GameObject dropPrefab;

    [Tooltip("โอกาสดรอปของ (0 = ไม่ดรอปเลย, 100 = ดรอปแน่นอน)")]
    [Range(0f, 100f)]
    public float dropChance = 100f;

    [Tooltip("จุดดรอปของ (ยกแกน Y ขึ้นนิดนึง ของจะได้ไม่จมดินตอนเกิด)")]
    public Vector3 dropOffset = new Vector3(0f, 0.5f, 0f);

    // ฟังก์ชันนี้จะถูกเรียกใช้จาก HealthSystem เมื่อศัตรูตาย
    public void DropItem()
    {
        // Debug เพื่อเช็คว่าฟังก์ชันถูกเรียกไหม
        Debug.Log($"<color=yellow>[ItemDrop]</color> {gameObject.name} กำลังพยายามดรอปไอเท็ม...");

        if (dropPrefab == null)
        {
            Debug.LogError($"<color=red>[ItemDrop Error]</color> {gameObject.name} ไม่มี Prefab ในช่อง dropPrefab! (กรุณาลากใส่ใน Inspector)");
            return;
        }

        float randomValue = Random.Range(0f, 100f);

        if (randomValue <= dropChance)
        {
            // ดรอปสำเร็จ
            Instantiate(dropPrefab, transform.position + dropOffset, Quaternion.identity);
            Debug.Log($"<color=green>[ItemDrop Success]</color> {gameObject.name} ดรอปสำเร็จ! (สุ่มได้ {randomValue:F1} จากโอกาส {dropChance}%)");
        }
        else
        {
            // ดรอปไม่สำเร็จ (สุ่มไม่โดน)
            Debug.Log($"<color=white>[ItemDrop Failed]</color> {gameObject.name} ไม่ดรอปไอเท็ม (สุ่มได้ {randomValue:F1} ซึ่งมากกว่าโอกาส {dropChance}%)");
        }
    }
}