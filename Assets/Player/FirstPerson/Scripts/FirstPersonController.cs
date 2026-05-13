namespace EasyPeasyFirstPersonController
{
    using UnityEngine;

    public partial class FirstPersonController : MonoBehaviour
    {
        [Header("Settings")]
        public float walkSpeed = 3f;
        public float sprintSpeed = 5f;
        public float crouchSpeed = 1.5f;
        public float jumpSpeed = 4f;
        public float gravity = 9.81f;
        public float slideDuration = 0.7f;
        public float slideSpeed = 6f;
        public float mouseSensitivity = 2f;
        public float strafeTiltAmount = 2f;

        [Header("References")]
        public Transform playerCamera;
        public Transform cameraParent;
        public Transform groundCheck;
        public LayerMask groundMask;

        [HideInInspector] public CharacterController characterController;
        [HideInInspector] public IInputManager input;
        [HideInInspector] public Vector3 moveDirection;
        [HideInInspector] public bool isGrounded;

        // --- ส่วนที่เพิ่มเข้ามาเพื่อแก้ปัญหาการกระโดด ---
        [HideInInspector] public float jumpCooldown = 0f;
        private float groundedCheckCooldown = 0f;
        // ------------------------------------------

        private PlayerBaseState currentState;
        private PlayerStateFactory states;
        private float xRotation = 0f;
        private float currentTilt;
        private float tiltVelocity;

        public PlayerBaseState CurrentState { get => currentState; set => currentState = value; }

        [Header("Visual Settings")]
        public float normalFov = 60f;
        public float sprintFov = 75f;
        public float slideFovBoost = 5f;
        public float fovChangeSpeed = 8f;
        public float bobAmount = 0.001f;
        public float bobSpeed = 10f;
        public float recoilReturnSpeed = 5f;

        [HideInInspector] public Camera cam;
        [HideInInspector] public float targetFov;
        [HideInInspector] public float currentBobIntensity;
        [HideInInspector] public float currentBobSpeed;
        [HideInInspector] public float targetTilt;

        private float bobTimer;
        private float fovVelocity;
        private float originalCamY;

        [Header("Height Settings")]
        public float standingCameraHeight = 1.75f;
        public float crouchingCameraHeight = 1f;
        public float crouchingCharacterControllerHeight = 1f;
        [HideInInspector] public float standingCharacterControllerHeight = 1.8f;
        [HideInInspector] public Vector3 standingCharacterControllerCenter = new Vector3(0, 0.9f, 0);
        [HideInInspector] public float targetCameraY;

        [Header("Ledge Settings")]
        public LayerMask ledgeLayer;
        public float ledgeDetectionDistance = 1f;
        private float landingMomentum;

        [Header("Swimming Settings")]
        public float swimSpeed = 4f;
        public float swimSprintSpeed = 6f;
        public float waterDrag = 2f;
        public LayerMask waterMask;
        [HideInInspector] public bool isInWater;

        [Header("Visual Preferences")]
        public bool useFovKick = true;
        public bool useHeadBob = true;
        public bool useCameraTilt = true;
        public bool useClimbTilt = true;

        [Header("Debug")]
        public bool currentStateDebug = true;

        void OnGUI()
        {
            if (currentState != null && Application.isEditor && currentStateDebug)
                GUILayout.Label("Current State: " + currentState.GetType().Name);
        }

        private void Awake()
        {
            cam = playerCamera.GetComponent<Camera>();
            targetFov = normalFov;
            targetCameraY = standingCameraHeight;
            originalCamY = standingCameraHeight;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            characterController = GetComponent<CharacterController>();
            standingCharacterControllerHeight = characterController.height;
            standingCharacterControllerCenter = characterController.center;
            input = GetComponent<IInputManager>();
            states = new PlayerStateFactory(this);

            currentState = states.Grounded();
            currentState.EnterState();
        }

        private float airTime = 0f; // เพิ่มตัวแปรนี้ไว้นอก Update

        private void Update()
        {
            // --- ระบบเช็คพื้นและหน่วงเวลา (Jump Lock) ---
            if (groundCheck != null)
            {
                // ถ้าเรากำลังกระโดด (อากาศ) ให้เพิ่มเวลา airTime
                // ถ้าเราอยู่บนพื้น ให้เช็คว่าเราลอยมานานพอหรือยังถึงจะอนุญาตให้แตะพื้น
                bool currentlyGrounded = Physics.CheckSphere(groundCheck.position, 0.25f, groundMask, QueryTriggerInteraction.Ignore);

                if (currentlyGrounded)
                {
                    airTime += Time.deltaTime;
                    // ถ้าแตะพื้นมานานเกิน 0.2 วินาที ถึงจะยอมให้สถานะ isGrounded เป็น True
                    if (airTime > 0.2f)
                    {
                        isGrounded = true;
                    }
                }
                else
                {
                    isGrounded = false;
                    airTime = 0f; // ลอยอยู่ ให้รีเซ็ตเวลา
                }
            }
            else { isGrounded = true; }

            // --- รัน State Machine ต่อตามปกติ ---
            currentState.UpdateState();
            HandleRotation();
            UpdateVisuals();
        }

        private void HandleRotation()
        {
            float mouseX = input.lookInput.x * mouseSensitivity;
            float mouseY = input.lookInput.y * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            float strafeTilt = useCameraTilt ? (-input.moveInput.x * strafeTiltAmount) : 0;
            float combinedTargetTilt = (useCameraTilt ? targetTilt : 0) + strafeTilt;

            currentTilt = Mathf.SmoothDamp(currentTilt, combinedTargetTilt, ref tiltVelocity, 0.1f);
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0, currentTilt);
        }

        public void UpdateVisuals()
        {
            if (!useFovKick) targetFov = normalFov;
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetFov, ref fovVelocity, 1f / fovChangeSpeed);

            landingMomentum = Mathf.Lerp(landingMomentum, 0, Time.deltaTime * 10f);
            float newY = Mathf.Lerp(cameraParent.localPosition.y, targetCameraY, Time.deltaTime * 8f);

            if (useHeadBob && characterController.velocity.magnitude > 0.1f && isGrounded)
            {
                bobTimer += Time.deltaTime * currentBobSpeed;
                float bobOffset = Mathf.Sin(bobTimer) * currentBobIntensity;
                cameraParent.localPosition = new Vector3(cameraParent.localPosition.x, newY + bobOffset, cameraParent.localPosition.z);
            }
            else
            {
                bobTimer = 0;
                cameraParent.localPosition = new Vector3(cameraParent.localPosition.x, newY, cameraParent.localPosition.z);
            }
        }

        // ... (ส่วนที่เหลือของโค้ดเดิมของคุณ HasCeiling, CheckLedge, OnTriggerEnter, OnTriggerExit คงเดิมไว้ได้เลยครับ)
        public bool HasCeiling() { /* ... */ return false; }
        public bool CheckLedge(out Vector3 climbPosition) { climbPosition = Vector3.zero; return false; }
        private void OnTriggerEnter(Collider other) { /* ... */ }
        private void OnTriggerExit(Collider other) { /* ... */ }
    }
}