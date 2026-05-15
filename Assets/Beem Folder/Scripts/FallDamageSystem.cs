using UnityEngine;

[RequireComponent(typeof(HealthSystem))] // บังคับว่าต้องมีสคริปต์เลือดอยู่ด้วย
public class FallDamageSystem : MonoBehaviour
{
    [Header("=== Fall Damage Settings ===")]
    [Tooltip("ระยะความสูงที่ตกแล้วปลอดภัย (เช่น ตกไม่เกิน 5 เมตรจะไม่ลดเลือด)")]
    public float safeFallDistance = 5f;
    [Tooltip("ดาเมจที่จะโดน ต่อทุกๆ 1 เมตรที่เกินจากระยะปลอดภัย")]
    public float fallDamageMultiplier = 10f;

    [Header("=== Ground Check Settings ===")]
    [Tooltip("ลาก Object ที่อยู่ที่เท้า (เช่น GroundCheckE) มาใส่")]
    public Transform groundCheck;
    [Tooltip("เลเยอร์ของพื้น (Ground)")]
    public LayerMask groundMask;
    public float groundCheckRadius = 0.3f;

    private float highestPointInAir;
    private bool wasGrounded;
    private HealthSystem healthSystem;

    private void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
        highestPointInAir = transform.position.y;
        wasGrounded = true;
    }

    private void Update()
    {
        if (groundCheck == null || healthSystem == null) return;

        // เช็คว่าเท้าแตะพื้นอยู่หรือไม่
        bool isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);

        if (!isGrounded)
        {
            // ถ้าลอยอยู่กลางอากาศ ให้จดจำตำแหน่ง Y ที่สูงที่สุดเอาไว้
            if (transform.position.y > highestPointInAir)
            {
                highestPointInAir = transform.position.y;
            }
        }
        else
        {
            // จังหวะที่เท้าเพิ่งแตะพื้นปุ๊บ (ตกถึงพื้นเฟรมแรก)
            if (!wasGrounded)
            {
                float fallDistance = highestPointInAir - transform.position.y;

                // ถ้าตกเกินระยะปลอดภัย ให้ทำดาเมจ
                if (fallDistance > safeFallDistance)
                {
                    float damage = (fallDistance - safeFallDistance) * fallDamageMultiplier;

                    healthSystem.TakeDamage(damage);

                    // ถ้าตัวที่ตกเป็นตัวที่เราสิงอยู่ ให้โชว์จอแดงวาบ
                    if (gameObject.CompareTag("Player") && PlayerHUD.instance != null)
                    {
                        PlayerHUD.instance.ShowDamageEffect();
                    }
                }
            }

            // ถ้ายืนอยู่บนพื้น ให้รีเซ็ตจุดสูงสุดเป็นตำแหน่งปัจจุบัน
            highestPointInAir = transform.position.y;
        }

        // เก็บสถานะไว้ใช้เทียบในเฟรมถัดไป
        wasGrounded = isGrounded;
    }
}