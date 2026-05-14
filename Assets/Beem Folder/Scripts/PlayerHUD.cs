using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD instance; // ใช้ Singleton เพื่อให้ทุกร่างส่งค่ามาหา UI นี้ได้ง่ายๆ

    [Header("=== Health Bar UI ===")]
    public Image healthFill;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = currentHealth / maxHealth;
        }
    }
}