using UnityEngine;
using UnityEngine.AI;

public class EnemyMain : MonoBehaviour
{
    [Header("=== Health Settings ===")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("=== Character Roles ===")]
    public bool isMainCharacter = false;
    public bool isInvincibleWhileEmpty = true;
    [Tooltip("ติ๊กถูกถ้าตัวนี้คือ Boss")]
    public bool isBoss = false;

    [Header("=== UI Settings ===")]
    public FloatingHealthBar floatingHealthBar;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        UpdateUI();
    }

    public void TakeDamage(float damageAmount)
    {
        // 1. ตรวจสอบเบื้องต้น
        if (currentHealth <= 0) return;

        // 2. ปรับเงื่อนไขอมตะให้ปลอดภัยขึ้น (เช็คชื่อร่างต้นแทนการเช็ค Tag)
        if (isMainCharacter && isInvincibleWhileEmpty)
        {
            // ถ้าเป็นร่างหลัก และเราไม่ได้สิงร่างนี้อยู่ (เช็คจาก PossessionSystem ว่าทำงานไหม)
            PossessionSystem ps = GetComponent<PossessionSystem>();
            if (ps != null && !ps.enabled)
            {
                Debug.Log("โจมตีไม่เข้า: ร่างต้นยังไม่มีคนสิง");
                return;
            }
        }

        // 3. ลดเลือดจริง
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"[HIT] {gameObject.name} เลือดลดเหลือ: {currentHealth}");

        UpdateUI();

        // 4. เช็คความตาย
        if (currentHealth <= 0) Die();
    }

    public void UpdateUI()
    {
        // อัปเดตหลอดเลือดบนหัว
        if (floatingHealthBar != null)
            floatingHealthBar.UpdateHealth(currentHealth, maxHealth);

        // อัปเดตหลอดเลือด HUD (ถ้าเราสิงอยู่)
        if (gameObject.CompareTag("Player") && PlayerHUD.instance != null)
            PlayerHUD.instance.UpdateHealth(currentHealth, maxHealth);

        // อัปเดตหลอดเลือดบอส
        if (isBoss && BossHealthUI.instance != null)
        {
            BossHealthUI.instance.UpdateHealth(currentHealth, maxHealth);
            if (currentHealth < maxHealth) BossHealthUI.instance.SetBossUIActive(true);
        }
    }

    private void Die()
    {
        Debug.Log($"<color=black><b>{gameObject.name} ตายแล้ว!</b></color>");

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.SetTrigger("Die");

        // ปิด AI ทุกรูปแบบ
        if (GetComponent<EnemyAI>() != null) GetComponent<EnemyAI>().enabled = false;
        if (GetComponent<BossAI>() != null) GetComponent<BossAI>().enabled = false;

        // ปิด NavMesh
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // ปิด Collider
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // คืนร่างถ้าตายตอนสิง
        if (gameObject.CompareTag("Player"))
        {
            PossessionSystem ps = GetComponent<PossessionSystem>();
            if (ps != null) ps.ForceReturnToMainBody();
        }

        gameObject.tag = "Untagged";
        if (isBoss && BossHealthUI.instance != null) BossHealthUI.instance.SetBossUIActive(false);

        Destroy(gameObject, 4f);
    }
}