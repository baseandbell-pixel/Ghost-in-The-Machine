using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class BossAI : MonoBehaviour
{
    [Header("=== Boss AI & Movement ===")]
    public float chaseRange = 30f;
    public float attackRange = 15f;
    public float turnSpeed = 5f;

    [Header("=== Teleport Settings ===")]
    public Transform[] teleportPoints;
    public float teleportCooldown = 8f;
    private float teleportTimer;

    [Header("=== Combat (Burst Fire) ===")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int burstCount = 5;
    public float burstFireRate = 0.15f;
    public float attackCooldown = 3f;

    private float attackTimer;
    private bool isFiring = false;
    private bool isDead = false; // เช็คสถานะการตายของบอส

    private Transform playerTarget;
    private NavMeshAgent agent;
    private Animator anim; // ตัวแปรสำหรับคุมแอนิเมชัน

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // ค้นหา Animator ในโมเดลลูกของบอส
        anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        teleportTimer = teleportCooldown;
        attackTimer = attackCooldown;
    }

    private void Update()
    {
        if (isDead) return; // ถ้าตายแล้ว ไม่ต้องทำอะไรต่อ

        // 1. ถ้าบอสโดนสิง ให้ AI หยุดทำงาน
        if (gameObject.CompareTag("Player"))
        {
            if (agent.enabled) agent.isStopped = true;
            if (anim != null) anim.SetFloat("Speed", 0f);
            return;
        }

        // 2. ค้นหาผู้เล่น (และเช็คว่าผู้เล่นยังไม่ตาย)
        FindPlayer();
        if (playerTarget == null)
        {
            if (agent.enabled) agent.isStopped = true;
            if (anim != null) anim.SetFloat("Speed", 0f);
            return;
        }

        // 3. ระบบวาร์ป (ไม่วาร์ปตอนยิง)
        teleportTimer -= Time.deltaTime;
        if (teleportTimer <= 0 && !isFiring)
        {
            TeleportToRandomPoint();
            teleportTimer = teleportCooldown;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // 4. พฤติกรรมการต่อสู้
        if (distanceToPlayer <= attackRange)
        {
            // --- ระยะยิง ---
            if (agent.enabled) agent.isStopped = true;
            LookAtPlayer();

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0 && !isFiring)
            {
                StartCoroutine(FireBurst());
                attackTimer = attackCooldown;
            }
        }
        else if (distanceToPlayer <= chaseRange && !isFiring)
        {
            // --- ระยะวิ่งไล่ ---
            if (agent.enabled)
            {
                agent.isStopped = false;
                agent.SetDestination(playerTarget.position);
            }
        }
        else
        {
            // --- นอกระยะ ---
            if (agent.enabled && !isFiring) agent.isStopped = true;
        }

        // 5. ส่งค่าความเร็วไปให้ Animator (สำหรับท่าเดิน/วิ่งของบอส)
        if (anim != null && agent.enabled)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        // เช็คทั้ง Tag และดูว่า Target ยัง Active อยู่ไหม (ป้องกันยิงศพ)
        if (playerObj != null && playerObj.CompareTag("Player"))
            playerTarget = playerObj.transform;
        else
            playerTarget = null;
    }

    private void LookAtPlayer()
    {
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
        }
    }

    private void TeleportToRandomPoint()
    {
        if (teleportPoints == null || teleportPoints.Length == 0) return;
        int randomIndex = Random.Range(0, teleportPoints.Length);

        if (agent.enabled) agent.Warp(teleportPoints[randomIndex].position);
        else transform.position = teleportPoints[randomIndex].position;
    }

    private IEnumerator FireBurst()
    {
        isFiring = true;
        if (agent.enabled) agent.isStopped = true;

        for (int i = 0; i < burstCount; i++)
        {
            if (isDead) break; // ถ้าตายระหว่างยิง ให้หยุดทันที

            // สั่งเล่นแอนิเมชันท่ายิง (Fire Trigger)
            if (anim != null) anim.SetTrigger("Fire");

            if (bulletPrefab != null && firePoint != null)
            {
                Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            }
            yield return new WaitForSeconds(burstFireRate);
        }
        isFiring = false;
    }

    // ==========================================
    // 👇👇 ฟังก์ชันตายสำหรับบอส 👇👇
    // ==========================================
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. เล่นแอนิเมชันตาย
        if (anim != null) anim.SetTrigger("Die");

        // 2. หยุดทุกระบบ
        if (agent != null) agent.enabled = false;

        // 3. เปลี่ยน Tag เพื่อไม่ให้โดนยิงซ้ำ
        gameObject.tag = "Untagged";

        // 4. บังคับคืนร่างถ้าผู้เล่นสิงบอสตัวนี้อยู่
        PossessionSystem ps = GetComponent<PossessionSystem>();
        if (ps != null && ps.enabled)
        {
            ps.ForceReturnToMainBody();
        }

        // 5. บอสอาจจะทิ้งศพไว้นานหน่อย (เช่น 10 วินาที) หรือไม่ทำลายเลยก็ได้
        Destroy(gameObject, 10f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}