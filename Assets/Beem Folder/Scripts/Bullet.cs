using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 20f;
    public float damage = 10f;

    [Tooltip("Tag ของคนยิง เพื่อป้องกันไม่ให้กระสุนระเบิดใส่ตัวเองตอนกดยิง")]
    public string ignoreTag = "Player";

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter(Collider other)
    {
        ProcessHit(other.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        ProcessHit(collision.gameObject);
    }

    private void ProcessHit(GameObject hitObject)
    {
        // 1. [แก้ไขแล้ว] ป้องกัน Error โดยการเช็คก่อนว่าตัวแปร ignoreTag ไม่ได้ว่างเปล่า
        if (!string.IsNullOrEmpty(ignoreTag))
        {
            // ถ้ายิงไปโดนสิ่งที่มี Tag ตรงกับ ignoreTag ให้ข้ามไปเลย ไม่ทำอะไร
            if (hitObject.CompareTag(ignoreTag))
            {
                return;
            }
        }

        // 2. ถ้าไม่ได้โดนตัวเอง ก็มาเช็คว่าสิ่งที่โดนมี HealthSystem ไหม
        HealthSystem targetHealth = hitObject.GetComponent<HealthSystem>();

        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
        }

        // 3. ทำลายกระสุนทิ้งหลังจากการชน
        Destroy(gameObject);
    }
}