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

    // ฟังก์ชันนี้จะถูกเรียกใช้เมื่อศัตรูตาย
    public void DropItem()
    {
        if (dropPrefab != null)
        {
            float randomValue = Random.Range(0f, 100f);

            if (randomValue <= dropChance)
            {
                Instantiate(dropPrefab, transform.position + dropOffset, Quaternion.identity);
            }
        }
    }
}