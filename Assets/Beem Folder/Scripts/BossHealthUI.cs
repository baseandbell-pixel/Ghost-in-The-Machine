using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    public static BossHealthUI instance;

    [Header("=== Boss UI References ===")]
    [Tooltip("Panel กรอบหลอดเลือดบอสทั้งหมด (เพื่อสั่งเปิด/ปิด)")]
    public GameObject bossPanel;
    [Tooltip("หลอดเลือดที่ตั้งเป็น Filled")]
    public Image healthFill;

    private void Awake()
    {
        // ทำเป็น Singleton เพื่อให้บอสส่งข้อมูลมาหาได้ง่ายๆ
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // ปิดหลอดเลือดบอสซ่อนไว้ก่อนตอนเริ่มเกม
        if (bossPanel != null) bossPanel.SetActive(false);
    }

    // ฟังก์ชันสั่งเปิด/ปิด หลอดเลือด
    public void SetBossUIActive(bool isActive)
    {
        if (bossPanel != null && bossPanel.activeSelf != isActive)
        {
            bossPanel.SetActive(isActive);
        }
    }

    // ฟังก์ชันอัปเดตความยาวหลอดเลือด
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = currentHealth / maxHealth;
        }
    }
}