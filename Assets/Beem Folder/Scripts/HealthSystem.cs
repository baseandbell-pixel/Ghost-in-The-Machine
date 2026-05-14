using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [Header("=== Health Settings ===")]
    public float maxHealth = 100f;

    [Header("=== Main Player Settings ===")]
    public bool isMainCharacter = false;
    public bool isInvincibleWhileEmpty = true;

    [Header("=== UI Settings ===")]
    public FloatingHealthBar floatingHealthBar;

    [Header("=== Suicide Mechanic ===")]
    public KeyCode suicideKey = KeyCode.Q;
    public float autoDestroyTime = 5f;

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
        if (!isMainCharacter && gameObject.CompareTag("Player"))
        {
            GameObject[] remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (remainingEnemies.Length == 0)
            {
                if (Input.GetKeyDown(suicideKey)) TriggerSuicide();

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
            else { isCountingDown = false; }
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

        Debug.Log($"<color=orange>{gameObject.name} โดนดาเมจ!</color> เลือดเหลือ: {currentHealth}");

        UpdateUI();

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
    }

    private void Die()
    {
        // 👇👇 [เพิ่มใหม่] เช็คว่าตัวนี้มีสคริปต์ดรอปของไหม ถ้ามีให้ดรอปของก่อนตาย 👇👇
        if (!isMainCharacter)
        {
            EnemyItemDrop itemDrop = GetComponent<EnemyItemDrop>();
            if (itemDrop != null)
            {
                itemDrop.DropItem();
            }
        }

        if (isMainCharacter) Debug.Log("ร่างหลักตาย - GAME OVER!!!");
        else if (gameObject.CompareTag("Player"))
        {
            if (possessionSystem != null) possessionSystem.ForceReturnToMainBody();
            Destroy(gameObject);
        }
        else Destroy(gameObject);
    }
}