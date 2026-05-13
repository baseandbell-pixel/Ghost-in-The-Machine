using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform firePoint;
    public GameObject bulletPrefab;

    private Transform target;
    private float fireCooldown = 0f;

    void Update()
    {
        // --- ส่วนที่เพิ่มเข้ามา: ถ้าเราสิงร่างนี้อยู่ ให้ปิด AI ทิ้ง! ---
        // เราเช็คว่าถ้าสคริปต์ PossessionSystem ถูกปิดอยู่ (แปลว่าเราสิงร่างนี้) 
        // หรือถ้าตัวนี้ไม่มี Component ควบคุมจากผู้เล่น ก็ให้รัน AI ปกติ
        if (GetComponent<PossessionSystem>() != null && GetComponent<PossessionSystem>().enabled)
        {
            agent.isStopped = true;
            return; // หยุดทำงาน AI ทันที เพื่อให้เราคุมเอง
        }
        // ----------------------------------------------------

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) target = playerObj.transform;

        if (target != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);

            if (dist < 10f)
            {
                agent.isStopped = true;
                transform.LookAt(target);

                if (fireCooldown <= 0f)
                {
                    Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                    fireCooldown = 1f;
                }
                fireCooldown -= Time.deltaTime;
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(target.position);
            }
        }
    }
}