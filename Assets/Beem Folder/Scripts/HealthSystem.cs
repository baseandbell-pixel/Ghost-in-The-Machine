using UnityEngine;
using UnityEngine.AI;

public class HealthSystem : MonoBehaviour
{
    [Header("=== Health Settings ===")]
    public float maxHealth = 100f;

    [Header("=== Character Roles ===")]
    public bool isMainCharacter = false;
    public bool isInvincibleWhileEmpty = true;
    [Tooltip("ติ๊กถูกถ้าตัวนี้คือ Boss (เพื่อโชว์หลอดเลือดบนหน้าจอ)")]
    public bool isBoss = false;

    [Header("=== Suicide Mechanic ===")]
    public bool allowManualSuicide = false;
    public KeyCode suicideKey = KeyCode.Q;
    public bool allowAutoSuicide = false;
    public float autoDestroyTime = 5f;

    [Header("=== UI Settings ===")]
    public FloatingHealthBar floatingHealthBar;

    private float destroyTimer;
    private bool isCountingDown = false;

    public float currentHealth { get; private set; }
    private PossessionSystem possessionSystem;

    private void Start()
    {
        currentHealth = maxHealth;
        possessionSystem = GetComponent<PossessionSystem>();
        UpdateUI();
    }

    private void Update()
    {
        if (!allowManualSuicide && !allowAutoSuicide) return;

        if (!isMainCharacter && gameObject.CompareTag("Player"))
        {
            GameObject[] remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (remainingEnemies.Length == 0)
            {
                if (allowManualSuicide && Input.GetKeyDown(suicideKey))
                {
                    TriggerSuicide();
                }

                if (allowAutoSuicide)
                {
                    if (!isCountingDown)
                    {
                        isCountingDown = true;
                        destroyTimer = autoDestroyTime;
                    }
                    else
                    {
                        destroyTimer -= Time.deltaTime;
                        if (destroyTimer <= 0) TriggerSuicide();
                    }
                }
                else
                {
                    isCountingDown = false;
                }
            }
            else
            {
                isCountingDown = false;
            }
        }
    }

    private void TriggerSuicide()
    {
        currentHealth = 0;
        UpdateUI();
        Die();
    }

    public void TakeDamage(float damageAmount)
    {
        if (isMainCharacter && !gameObject.CompareTag("Player") && isInvincibleWhileEmpty) return;

        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        UpdateUI();

        if (gameObject.CompareTag("Player") && PlayerHUD.instance != null)
        {
            PlayerHUD.instance.ShowDamageEffect();
        }

        if (currentHealth <= 0) Die();
    }

    public void UpdateUI()
    {
        if (floatingHealthBar != null)
        {
            floatingHealthBar.UpdateHealth(currentHealth, maxHealth);
        }

        if (gameObject.CompareTag("Player") && PlayerHUD.instance != null)
        {
            PlayerHUD.instance.UpdateHealth(currentHealth, maxHealth);
        }

        if (isBoss && BossHealthUI.instance != null)
        {
            BossHealthUI.instance.UpdateHealth(currentHealth, maxHealth);
            if (currentHealth < maxHealth && currentHealth > 0)
            {
                BossHealthUI.instance.SetBossUIActive(true);
            }
        }
    }

    private void Die()
    {
        // --- 1. สั่งดรอปไอเท็ม (เพิ่มส่วนนี้เข้าไป) ---
        EnemyItemDrop itemDrop = GetComponent<EnemyItemDrop>();
        if (itemDrop != null)
        {
            itemDrop.DropItem();
        }

        // --- 2. จัดการ Animator ---
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // --- 3. ปิดการทำงาน AI และ NavMesh (แก้ไขป้องกัน Error) ---
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // --- 4. จัดการระบบสิงร่างและกล้อง ---
        if (gameObject.CompareTag("Player"))
        {
            PossessionSystem ps = GetComponent<PossessionSystem>();
            if (ps != null) ps.ForceReturnToMainBody();

            if (Camera.main != null && Camera.main.transform.IsChildOf(this.transform))
            {
                Camera.main.transform.SetParent(null);
            }
        }

        // --- 5. ล้างสถานะและทำลาย Object ---
        gameObject.tag = "Untagged";
        if (floatingHealthBar != null) floatingHealthBar.gameObject.SetActive(false);

        Destroy(gameObject, 4f);
    }
}