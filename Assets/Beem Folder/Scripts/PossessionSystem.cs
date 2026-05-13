using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using EasyPeasyFirstPersonController;

public class PossessionSystem : MonoBehaviour
{
    [Header("Possession Settings")]
    public float possessionTimeRequired = 2f;
    public float maxLookDistance = 15f;
    public LayerMask enemyLayer;
    public string playerLayerName = "Player";
    public KeyCode aimKey = KeyCode.Mouse1;
    public KeyCode possessionKey = KeyCode.E;

    [Header("References")]
    public Transform playerCamera;
    public Image progressBar;
    public GameObject pressEText;

    private float currentLookTime = 0f;
    private bool isReadyToPossess = false;
    private GameObject targetEnemy;

    [HideInInspector] public GameObject previousBody;
    private FirstPersonController fpsController;
    private PlayerShooting shootingSystem;

    private void Awake()
    {
        fpsController = GetComponent<FirstPersonController>();
        shootingSystem = GetComponent<PlayerShooting>();
        if (fpsController != null && !fpsController.enabled)
        {
            this.enabled = false;
            if (shootingSystem != null) shootingSystem.enabled = false;
        }
    }

    private void Update()
    {
        if (previousBody != null && Input.GetKeyDown(possessionKey) && !Input.GetKey(aimKey))
        {
            ExecuteTransfer(previousBody, true);
        }
        else { HandlePossessionLogic(); }
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
                    else if (Input.GetKeyDown(possessionKey)) { ExecuteTransfer(targetEnemy, false); }
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
        if (progressBar != null) { progressBar.fillAmount = 1f; progressBar.gameObject.SetActive(false); }
        if (pressEText != null) pressEText.SetActive(false);
    }

    private void ExecuteTransfer(GameObject newBody, bool isReturning)
    {
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject b in bullets) { Destroy(b); }

        FirstPersonController newBodyFPS = newBody.GetComponent<FirstPersonController>();
        PossessionSystem newBodyPossession = newBody.GetComponent<PossessionSystem>();
        PlayerShooting newBodyShooting = newBody.GetComponent<PlayerShooting>();
        CharacterController newBodyChar = newBody.GetComponent<CharacterController>();

        NavMeshAgent newBodyNavAgent = newBody.GetComponent<NavMeshAgent>();
        EnemyAI newBodyAI = newBody.GetComponent<EnemyAI>();

        newBody.tag = "Player";
        this.gameObject.tag = "Untagged";

        // --- [เพิ่มใหม่] ส่งต่อข้อมูล กล้อง และ UI ไปให้สคริปต์ของร่างใหม่ ---
        // เพื่อให้ร่างใหม่รู้ว่าต้องใช้กล้องตัวไหนและ UI ตัวไหนตอนที่มันเป็นคนคุม
        newBodyPossession.playerCamera = this.playerCamera;
        newBodyPossession.progressBar = this.progressBar;
        newBodyPossession.pressEText = this.pressEText;

        // ย้ายกล้อง
        playerCamera.SetParent(newBodyFPS.cameraParent);
        playerCamera.localPosition = Vector3.zero;
        playerCamera.localRotation = Quaternion.identity;

        if (!isReturning) newBodyPossession.previousBody = this.gameObject;
        else newBodyPossession.previousBody = null;

        newBody.layer = LayerMask.NameToLayer(playerLayerName);

        // ปิด AI ร่างใหม่
        if (newBodyNavAgent != null) newBodyNavAgent.enabled = false;
        if (newBodyAI != null) newBodyAI.enabled = false;

        // เปิดร่างใหม่
        newBodyFPS.enabled = true;
        newBodyPossession.enabled = true;
        if (newBodyShooting != null) newBodyShooting.enabled = true;

        // จัดการ Physics ร่างใหม่
        if (newBodyChar != null)
        {
            newBodyChar.enabled = false;
            newBodyChar.enabled = true;
            newBodyChar.Move(Vector3.up * 0.1f);
        }
        newBodyFPS.jumpCooldown = 0.5f;

        // ปิดร่างเก่า
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