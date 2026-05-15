using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD instance;

    [Header("=== Health Bar UI ===")]
    public Image healthFill;

    [Header("=== Damage Overlay Effect ===")]
    [Tooltip("ลาก Image จอแดง (DamageOverlay) มาใส่")]
    public Image damageOverlay;
    [Tooltip("ความเร็วในการวาบขึ้น (Attack Speed) ยิ่งค่าน้อยยิ่งวาบเร็ว")]
    public float flashDuration = 0.1f;
    [Tooltip("ความเร็วในการเฟดหาย (Fade Out Speed) ยิ่งมากยิ่งหายไว")]
    public float fadeSpeed = 3f;
    [Tooltip("ความแดงสูงสุดตอนโดนดาเมจ (0-1) แนะนำที่ 0.3 ถึง 0.5")]
    public float maxAlpha = 0.4f;

    private Color overlayColor;
    private Coroutine flashCoroutine; // เก็บอ้างอิงของ Coroutine ที่กำลังรันอยู่

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ทำให้ HUD ติดตามไปทุก Scene
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // รีเซ็ตจอแดงเป็นโปร่งใสตอนเริ่มเกม
        if (damageOverlay != null)
        {
            overlayColor = damageOverlay.color;
            overlayColor.a = 0f;
            damageOverlay.color = overlayColor;
        }
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = currentHealth / maxHealth;
        }
    }

    // ฟังก์ชันตะโกนสั่งให้จอแดง
    public void ShowDamageEffect()
    {
        if (damageOverlay != null)
        {
            // ถ้า Coroutine เก่ายังทำงานอยู่ ให้หยุดก่อนเพื่อให้เริ่มอันใหม่ทันที
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);

            // เริ่มต้นแอนิเมชันวาบแดง
            flashCoroutine = StartCoroutine(FlashDamageOverlay());
        }
    }

    // Coroutine จัดการแอนิเมชันวาบแดง
    private IEnumerator FlashDamageOverlay()
    {
        float timer = 0;

        // วาบขึ้น (Fade In)
        while (timer < flashDuration)
        {
            timer += Time.deltaTime;
            overlayColor.a = Mathf.Lerp(overlayColor.a, maxAlpha, timer / flashDuration);
            damageOverlay.color = overlayColor;
            yield return null; // รอเฟรมถัดไป
        }

        // ค่อยๆ หายไป (Fade Out)
        while (overlayColor.a > 0f)
        {
            overlayColor.a -= fadeSpeed * Time.deltaTime;
            damageOverlay.color = overlayColor;
            yield return null;
        }

        // บังคับปิดท้ายเป็นศูนย์
        overlayColor.a = 0f;
        damageOverlay.color = overlayColor;
        flashCoroutine = null; // เคลียร์ Reference
    }
}