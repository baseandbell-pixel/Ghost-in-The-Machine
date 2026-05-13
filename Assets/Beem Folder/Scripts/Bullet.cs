using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // ทำให้กระสุนพุ่งไปข้างหน้าทันทีด้วยความเร็วคงที่
        rb.velocity = transform.forward * speed;

        // ลบตัวเองทิ้งใน 3 วินาที เพื่อไม่ให้เปลือง Memory
        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter(Collider other)
    {
        // ถ้ากระสุนชนใคร (ยกเว้นคนยิง) ให้ทำลายตัวเอง
        if (!other.CompareTag("Enemy"))
        {
            // ใส่ฟังก์ชันลดเลือดที่นี่ (เช่น other.GetComponent<Health>().TakeDamage(10);)
            Destroy(gameObject);
        }
    }
}