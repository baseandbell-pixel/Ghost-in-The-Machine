using UnityEngine;
using System.Collections.Generic;

public class DamageArea : MonoBehaviour
{
    [Header("=== Acid / Lava Settings ===")]
    [Tooltip("ดาเมจที่จะลดแต่ละครั้ง")]
    public float damageAmount = 5f;
    [Tooltip("ความถี่ในการโดนดาเมจ (เช่น 0.5 คือโดนทุกๆ ครึ่งวินาที)")]
    public float damageInterval = 0.5f;

    [Tooltip("ให้ทำดาเมจใส่ใครบ้าง (ปกติคือ Player)")]
    public string targetTag = "Player";

    // ระบบสมุดจดบันทึก ว่าตัวละครแต่ละตัว โดนดาเมจครั้งสุดท้ายไปเมื่อไหร่
    private Dictionary<Collider, float> nextDamageTime = new Dictionary<Collider, float>();

    private void OnTriggerStay(Collider other)
    {
        // เช็คว่าคนที่เดินเหยียบ มี Tag ตรงกับที่ตั้งไว้ไหม (Player)
        if (other.CompareTag(targetTag))
        {
            // ถ้าไม่เคยจดชื่อไว้ หรือ ถึงเวลาที่ต้องโดนดาเมจรอบต่อไปแล้ว
            if (!nextDamageTime.ContainsKey(other) || Time.time >= nextDamageTime[other])
            {
                HealthSystem targetHealth = other.GetComponent<HealthSystem>();

                if (targetHealth != null)
                {
                    // สั่งทำดาเมจ
                    targetHealth.TakeDamage(damageAmount);

                    // จดเวลาไว้ว่า จะโดนดาเมจครั้งต่อไปตอนไหน
                    nextDamageTime[other] = Time.time + damageInterval;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // พอเดินออกจากน้ำกรด ให้ลบชื่อออกจากสมุดจด
        if (nextDamageTime.ContainsKey(other))
        {
            nextDamageTime.Remove(other);
        }
    }
}