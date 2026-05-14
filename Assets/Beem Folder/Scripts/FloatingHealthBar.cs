using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [Header("=== UI References ===")]
    public Image healthFill;
    public Canvas canvas; // เอาไว้สั่งปิดหลอดเลือดตอนที่เราสิงร่างนี้

    private Transform mainCameraTransform;

    private void Start()
    {
        if (Camera.main != null) mainCameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        // ทำให้หลอดเลือดบนหัวหันหน้าเข้าหากล้อง (ผู้เล่น) เสมอ
        if (mainCameraTransform != null)
        {
            transform.LookAt(transform.position + mainCameraTransform.forward);
        }
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthFill != null) healthFill.fillAmount = currentHealth / maxHealth;
    }

    public void SetVisible(bool isVisible)
    {
        if (canvas != null) canvas.enabled = isVisible;
    }
}