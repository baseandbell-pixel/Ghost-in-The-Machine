using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("=== AI Settings ===")]
    public NavMeshAgent agent;
    public Transform firePoint;
    public GameObject bulletPrefab;

    private Transform target;
    private float fireCooldown = 0f;
    private Animator anim;

    // ตัวแปรเช็คสถานะการตาย เพื่อป้องกันไม่ให้ AI ทำงานซ้ำซ้อนตอนเป็นศพ
    private bool isDead = false;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // 1. ถ้าศัตรูตัวนี้ตายแล้ว ให้หยุดการทำงานของ Update ทันที
        if (isDead) return;

        // 2. ถ้าเราสิงร่างนี้อยู่ ให้ปิด AI ทิ้ง!
        if (GetComponent<PossessionSystem>() != null && GetComponent<PossessionSystem>().enabled)
        {
            if (agent.enabled) agent.isStopped = true;
            if (anim != null) anim.SetFloat("Speed", 0f);
            return;
        }

        // 3. ค้นหา Player และเช็คว่าเป้าหมายยังมีชีวิตอยู่ (Tag ยังเป็น Player อยู่ ไม่ใช่ Untagged)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null && playerObj.CompareTag("Player"))
        {
            target = playerObj.transform;
        }
        else
        {
            target = null; // ถ้าหาไม่เจอ หรือผู้เล่นตายแล้ว ให้เคลียร์เป้าหมาย
        }

        // 4. พฤติกรรมการเดินและการยิง
        if (target != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);

            if (dist < 10f)
            {
                // อยู่ในระยะยิง
                if (agent.enabled) agent.isStopped = true;
                transform.LookAt(target);

                if (fireCooldown <= 0f)
                {
                    if (anim != null) anim.SetTrigger("Fire");
                    Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                    fireCooldown = 1f;
                }
                fireCooldown -= Time.deltaTime;
            }
            else
            {
                // เดินไล่ตาม
                if (agent.enabled)
                {
                    agent.isStopped = false;
                    agent.SetDestination(target.position);
                }
            }

            // ส่งค่าความเร็วให้ Animator
            if (anim != null && agent.enabled)
            {
                anim.SetFloat("Speed", agent.velocity.magnitude);
            }
        }
        else
        {
            // ถ้าไม่มีเป้าหมาย ให้ยืนนิ่งๆ
            if (agent.enabled) agent.isStopped = true;
            if (anim != null) anim.SetFloat("Speed", 0f);
        }
    }

    // ==========================================
    // 👇👇 ฟังก์ชันใหม่: เรียกใช้ฟังก์ชันนี้ตอนศัตรู HP = 0 👇👇
    // ==========================================
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. สั่งเล่นแอนิเมชันตาย
        if (anim != null) anim.SetTrigger("Die");

        // 2. ปิดระบบเดินและการชน (ศพจะได้ไม่บังทางเดิน)
        if (agent != null) agent.enabled = false;

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // 3. เปลี่ยน Tag เป็น Untagged (AI ตัวอื่นจะได้เลิกยิงศพนี้)
        gameObject.tag = "Untagged";

        // 4. ถ้าเรากำลังสิงศัตรูตัวนี้อยู่ตอนมันตาย ให้บังคับดีดวิญญาณกลับร่างหลัก
        PossessionSystem ps = GetComponent<PossessionSystem>();
        if (ps != null && ps.enabled)
        {
            ps.ForceReturnToMainBody();
        }

        // 5. ปิดหลอดเลือดบนหัว (ถ้ามี GameObject หลอดเลือดอยู่ข้างใน)
        // คุณสามารถอ้างอิงและ .SetActive(false) ตรงนี้ได้เลย

        // 6. ลบ Object ทิ้งหลังผ่านไป 4 วินาที (เพื่อให้เล่นแอนิเมชันตายจนจบก่อน)
        Destroy(gameObject, 4f);
    }
}