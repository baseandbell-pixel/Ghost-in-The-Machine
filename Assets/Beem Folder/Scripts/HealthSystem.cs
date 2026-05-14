using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;

    [Header("Main Player Settings")]
    [Tooltip("ติ๊กถูกเฉพาะที่ตัวละครหลัก (Player) ของเราเท่านั้น")]
    public bool isMainCharacter = false;

    [Tooltip("อยากให้ร่างหลักเป็นอมตะ ไม่รับดาเมจตอนที่เราไปสิงร่างอื่นหรือไม่?")]
    public bool isInvincibleWhileEmpty = true;

    [Header("=== Suicide Mechanic ===")]
    [Tooltip("ปุ่มสำหรับกดทำลายร่างตัวเองทันที")]
    public KeyCode suicideKey = KeyCode.Q;
    [Tooltip("เวลาที่จะนับถอยหลังก่อนระเบิดอัตโนมัติ (วินาที)")]
    public float autoDestroyTime = 5f;

    private float destroyTimer;
    private bool isCountingDown = false;

    private float currentHealth;
    private PossessionSystem possessionSystem;

    private void Start()
    {
        currentHealth = maxHealth;
        possessionSystem = GetComponent<PossessionSystem>();
    }

    private void Update()
    {
        // ทำงานเฉพาะตอนที่เรากำลัง "สิงร่างศัตรู" อยู่เท่านั้น
        // (ไม่ใช่ตัวหลัก และ Tag เปลี่ยนเป็น Player แล้ว)
        if (!isMainCharacter && gameObject.CompareTag("Player"))
        {
            // ค้นหาศัตรูที่เหลือในฉาก (ร่างที่เราสิงอยู่จะไม่ถูกนับ เพราะ Tag เป็น Player ไปแล้ว)
            GameObject[] remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy");

            // ถ้าไม่เหลือศัตรูตัวอื่นในฉากแล้ว
            if (remainingEnemies.Length == 0)
            {
                // เงื่อนไขที่ 1: กด Q เพื่อทำลายตัวเองทันที
                if (Input.GetKeyDown(suicideKey))
                {
                    Debug.Log("ผู้เล่นกด Q สละร่างสิงทันที!");
                    TriggerSuicide();
                }

                // เงื่อนไขที่ 2: เริ่มนับถอยหลัง 5 วินาที
                if (!isCountingDown)
                {
                    isCountingDown = true;
                    destroyTimer = autoDestroyTime;
                    Debug.Log($"เหลือศัตรูตัวสุดท้าย! เริ่มนับถอยหลัง {autoDestroyTime} วินาทีเพื่อทำลายร่าง...");
                }
                else
                {
                    // ลดเวลาลงตามเฟรมเรท
                    destroyTimer -= Time.deltaTime;

                    // สมมติถ้าอยากเอาเวลาไปโชว์ที่ UI สามารถส่งค่า destroyTimer ไปที่ Canvas ได้ตรงนี้

                    if (destroyTimer <= 0)
                    {
                        Debug.Log("หมดเวลา 5 วินาที! ร่างสิงระเบิดตัวเองอัตโนมัติ!");
                        TriggerSuicide();
                    }
                }
            }
            else
            {
                // ถ้ายังมีศัตรูตัวอื่นเหลืออยู่ ให้ยกเลิกการนับถอยหลัง (เผื่อมีบั๊กศัตรูเกิดใหม่)
                isCountingDown = false;
            }
        }
    }

    // ฟังก์ชันสั่งตายแบบทันที
    private void TriggerSuicide()
    {
        currentHealth = 0;
        Die(); // เรียกคำสั่งตาย เพื่อให้วาร์ปกลับร่างหลักและลบศัตรูตัวนี้ทิ้ง
    }

    public void TakeDamage(float damageAmount)
    {
        if (isMainCharacter && !gameObject.CompareTag("Player") && isInvincibleWhileEmpty)
        {
            Debug.Log("ร่างหลักเป็นอมตะอยู่ กระสุนทะลุ/ไม่รับดาเมจ!");
            return;
        }

        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} โดนยิง! เลือดเหลือ: {currentHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log($"*** {gameObject.name} เลือดเหลือ 0 แล้ว! ***");
            Die();
        }
    }

    private void Die()
    {
        if (isMainCharacter)
        {
            Debug.Log("ร่างหลักตาย - GAME OVER!!!");
            // gameObject.SetActive(false); 
        }
        else if (gameObject.CompareTag("Player"))
        {
            Debug.Log("ร่างสิงพัง! วาร์ปกลับร่างหลัก");
            if (possessionSystem != null)
            {
                possessionSystem.ForceReturnToMainBody();
            }
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("ศัตรูตาย!");
            Destroy(gameObject);
        }
    }
}