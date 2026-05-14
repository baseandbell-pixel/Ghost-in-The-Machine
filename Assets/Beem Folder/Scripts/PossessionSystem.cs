using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using EasyPeasyFirstPersonController;

public class PossessionSystem : MonoBehaviour
{
    [Header("=== Possession Settings ===")]
    [Tooltip("เวลาที่ต้องจ้องมองเป้าหมายเพื่อชาร์จพลังสิงร่าง (วินาที)")]
    public float possessionTimeRequired = 2f;
    [Tooltip("ระยะไกลสุดที่สามารถสิงเป้าหมายได้")]
    public float maxLookDistance = 15f;

    [Header("=== Layers & Tags ===")]
    [Tooltip("เลเยอร์ของศัตรูที่สามารถสิงได้")]
    public LayerMask enemyLayer;
    [Tooltip("ชื่อเลเยอร์ของฝั่งผู้เล่น (หลังสิงร่างจะถูกเปลี่ยนเป็นเลเยอร์นี้)")]
    public string playerLayerName = "Player";

    [Header("=== Key Binds ===")]
    [Tooltip("ปุ่มสำหรับเล็ง/เริ่มชาร์จพลังสิงร่าง")]
    public KeyCode aimKey = KeyCode.Mouse1;
    [Tooltip("ปุ่มกดยืนยันการสิงร่าง หรือวาร์ปกลับ")]
    public KeyCode possessionKey = KeyCode.E;

    [Header("=== UI & References ===")]
    [Tooltip("กล้องหลักของผู้เล่น")]
    public Transform playerCamera;
    [Tooltip("หลอด UI แสดงระยะเวลาการชาร์จสิงร่าง")]
    public Image progressBar;
    [Tooltip("ข้อความ UI แจ้งเตือนให้กดปุ่ม E")]
    public GameObject pressEText;

    // ----------------------------------------------------
    // [ตัวแปรซ่อน (Internal Variables)]
    // ----------------------------------------------------
    [HideInInspector] public GameObject previousBody;

    private float currentLookTime = 0f;
    private bool isReadyToPossess = false;
    private GameObject targetEnemy;
    private FirstPersonController fpsController;
    private PlayerShooting shootingSystem;

    private void Awake()
    {
        fpsController = GetComponent<FirstPersonController>();
        shootingSystem = GetComponent<PlayerShooting>();

        // ถ้าคอนโทรลเลอร์เดินปิดอยู่ (แปลว่าเป็นศัตรูที่ยังไม่ได้สิง) ให้ปิดระบบนี้รอไว้ก่อน
        if (fpsController != null && !fpsController.enabled)
        {
            this.enabled = false;
            if (shootingSystem != null) shootingSystem.enabled = false;
        }
    }

    private void Update()
    {
        // เงื่อนไขการวาร์ปกลับร่างเดิม
        if (previousBody != null && Input.GetKeyDown(possessionKey) && !Input.GetKey(aimKey))
        {
            ExecuteTransfer(previousBody, true);
        }
        else
        {
            HandlePossessionLogic();
        }
    }

    public void ForceReturnToMainBody()
    {
        if (previousBody != null)
        {
            ExecuteTransfer(previousBody, true);
        }
    }

    private void HandlePossessionLogic()
    {
        if (!this.enabled) return;

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxLookDistance, enemyLayer))
        {
            PossessionSystem targetSystem = hit.collider.GetComponent<PossessionSystem>();
            if (targetSystem != null && targetSystem != this && !targetSystem.enabled)
            {
                if (Input.GetKey(aimKey))
                {
                    if (!isReadyToPossess)
                    {
                        if (progressBar != null) progressBar.gameObject.SetActive(true);

                        currentLookTime += Time.deltaTime;

                        if (progressBar != null) progressBar.fillAmount = 1f - (currentLookTime / possessionTimeRequired);

                        if (currentLookTime >= possessionTimeRequired)
                        {
                            isReadyToPossess = true;
                            targetEnemy = targetSystem.gameObject;
                            if (progressBar != null) progressBar.gameObject.SetActive(false);
                            if (pressEText != null) pressEText.SetActive(true);
                        }
                    }
                    else if (Input.GetKeyDown(possessionKey))
                    {
                        ExecuteTransfer(targetEnemy, false);
                    }
                }
                else { ResetGaze(); }
            }
            else { ResetGaze(); }
        }
        else { ResetGaze(); }
    }

    private void ResetGaze()
    {
        currentLookTime = 0f;
        isReadyToPossess = false;
        targetEnemy = null;
        if (progressBar != null)
        {
            progressBar.fillAmount = 1f;
            progressBar.gameObject.SetActive(false);
        }
        if (pressEText != null) pressEText.SetActive(false);
    }

    private void ExecuteTransfer(GameObject newBody, bool isReturning)
    {
        // ล้างกระสุนที่ค้างอยู่ในฉาก
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject b in bullets) { Destroy(b); }

        // ดึง Component ของร่างใหม่
        FirstPersonController newBodyFPS = newBody.GetComponent<FirstPersonController>();
        PossessionSystem newBodyPossession = newBody.GetComponent<PossessionSystem>();
        PlayerShooting newBodyShooting = newBody.GetComponent<PlayerShooting>();
        CharacterController newBodyChar = newBody.GetComponent<CharacterController>();
        NavMeshAgent newBodyNavAgent = newBody.GetComponent<NavMeshAgent>();
        EnemyAI newBodyAI = newBody.GetComponent<EnemyAI>();

        // สลับ Tag
        newBody.tag = "Player";
        this.gameObject.tag = "Untagged";

        // ส่งต่อข้อมูลกล้องและ UI ให้สคริปต์ร่างใหม่
        newBodyPossession.playerCamera = this.playerCamera;
        newBodyPossession.progressBar = this.progressBar;
        newBodyPossession.pressEText = this.pressEText;

        // ส่งกล้องให้ Controller ร่างใหม่เพื่อป้องกัน Error หันกล้องไม่ได้
        if (newBodyFPS != null && this.playerCamera != null)
        {
            newBodyFPS.playerCamera = this.playerCamera;
            newBodyFPS.cam = this.playerCamera.GetComponent<Camera>();
        }

        // --- [จุดแก้ไขสำคัญเพื่อ Transition สวยงาม] ---
        // ใส่ค่า true เพื่อให้กล้องยังคงจำตำแหน่งเดิมบนโลกเอาไว้ 
        // ปล่อยให้สคริปต์ CameraSwitcher ทำหน้าที่ดึงกล้องไปหาร่างใหม่อย่างนุ่มนวล
        playerCamera.SetParent(newBodyFPS.cameraParent, true);

        // จัดการสถานะ Previous Body
        if (!isReturning) newBodyPossession.previousBody = this.gameObject;
        else newBodyPossession.previousBody = null;

        newBody.layer = LayerMask.NameToLayer(playerLayerName);

        // ปิด AI ร่างใหม่ และเปิดการบังคับ
        if (newBodyNavAgent != null) newBodyNavAgent.enabled = false;
        if (newBodyAI != null) newBodyAI.enabled = false;

        newBodyFPS.enabled = true;
        newBodyPossession.enabled = true;
        if (newBodyShooting != null) newBodyShooting.enabled = true;

        // ขยับ CharacterController ร่างใหม่นิดนึงกันจมพื้น
        if (newBodyChar != null)
        {
            newBodyChar.enabled = false;
            newBodyChar.enabled = true;
            newBodyChar.Move(Vector3.up * 0.1f);
        }
        newBodyFPS.jumpCooldown = 0.5f;

        // เปลี่ยนสถานะร่างเก่ากลับเป็นศัตรู
        this.gameObject.layer = LayerMask.NameToLayer("Enemy");
        this.fpsController.enabled = false;
        this.enabled = false;
        if (shootingSystem != null) shootingSystem.enabled = false;

        // รักษากล่อง Hitbox ร่างเก่า
        CharacterController oldChar = GetComponent<CharacterController>();
        if (oldChar != null) oldChar.enabled = true;

        // เปิด AI ร่างเก่า ให้กลับมาเดินเป็น AI
        NavMeshAgent oldBodyNavAgent = this.GetComponent<NavMeshAgent>();
        EnemyAI oldBodyAI = this.GetComponent<EnemyAI>();
        if (oldBodyNavAgent != null) oldBodyNavAgent.enabled = true;
        if (oldBodyAI != null) oldBodyAI.enabled = true;

        ResetGaze();
    }
}