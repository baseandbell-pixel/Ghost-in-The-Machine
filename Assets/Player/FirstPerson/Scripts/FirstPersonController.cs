namespace EasyPeasyFirstPersonController
{
    using UnityEngine;

    public partial class FirstPersonController : MonoBehaviour
    {
        [Header("=== Movement Settings ===")]
        [Tooltip("ความเร็วเดินปกติ")] public float walkSpeed = 3f;
        [Tooltip("ความเร็ววิ่ง")] public float sprintSpeed = 5f;
        [Tooltip("ความเร็วย่อตัวเดิน")] public float crouchSpeed = 1.5f;
        [Tooltip("ความแรงกระโดด")] public float jumpSpeed = 4f;
        [Tooltip("ระยะเวลาสไลด์")] public float slideDuration = 0.7f;
        [Tooltip("ความเร็วสไลด์")] public float slideSpeed = 6f;

        [Header("=== Physics & Environment ===")]
        [Tooltip("แรงโน้มถ่วง")] public float gravity = 9.81f;
        [Tooltip("เลเยอร์ของพื้น (Ground)")] public LayerMask groundMask;
        [Tooltip("เลเยอร์ของขอบกำแพง (Ledge)")] public LayerMask ledgeLayer;
        public float ledgeDetectionDistance = 1f;

        [Header("=== Water & Swimming ===")]
        public LayerMask waterMask;
        public float swimSpeed = 4f;
        public float swimSprintSpeed = 6f;
        public float waterDrag = 2f;

        [Header("=== Look & Rotation Settings ===")]
        [Tooltip("ความไวเมาส์")] public float mouseSensitivity = 2f;
        [Tooltip("ความเอียงของกล้องเวลาเดินข้าง")] public float strafeTiltAmount = 2f;

        [Header("=== Camera Visuals (FOV & Bobbing) ===")]
        public bool useFovKick = true;
        public bool useHeadBob = true;
        public bool useCameraTilt = true;
        public bool useClimbTilt = true;
        [Space]
        public float normalFov = 60f;
        public float sprintFov = 75f;
        public float slideFovBoost = 5f;
        public float fovChangeSpeed = 8f;
        public float bobAmount = 0.001f;
        public float bobSpeed = 10f;
        public float recoilReturnSpeed = 5f;

        [Header("=== Camera Heights ===")]
        public float standingCameraHeight = 1.75f;
        public float crouchingCameraHeight = 1f;
        public float crouchingCharacterControllerHeight = 1f;

        [Header("=== Core References ===")]
        public Transform playerCamera;
        public Transform cameraParent;
        public Transform groundCheck;

        [Header("=== Debug & States ===")]
        public bool currentStateDebug = true;

        // ----------------------------------------------------
        // [ตัวแปรที่ซ่อนไว้ (ใช้คำนวณในระบบหลังบ้าน ไม่ต้องโชว์ใน Inspector)]
        // ----------------------------------------------------
        [HideInInspector] public CharacterController characterController;
        [HideInInspector] public IInputManager input;
        [HideInInspector] public Vector3 moveDirection;
        [HideInInspector] public bool isGrounded;
        [HideInInspector] public float jumpCooldown = 0f;
        [HideInInspector] public Camera cam;
        [HideInInspector] public float targetFov;
        [HideInInspector] public float currentBobIntensity;
        [HideInInspector] public float currentBobSpeed;
        [HideInInspector] public float targetTilt;
        [HideInInspector] public float standingCharacterControllerHeight = 1.8f;
        [HideInInspector] public Vector3 standingCharacterControllerCenter = new Vector3(0, 0.9f, 0);
        [HideInInspector] public float targetCameraY;
        [HideInInspector] public bool isInWater;

        // ตัวแปร Private
        private PlayerBaseState currentState;
        private PlayerStateFactory states;
        private float xRotation = 0f;
        private float currentTilt;
        private float tiltVelocity;
        private float bobTimer;
        private float fovVelocity;
        private float originalCamY;
        private float landingMomentum;
        private float airTime = 0f;
        private float groundedCheckCooldown = 0f;

        public PlayerBaseState CurrentState { get => currentState; set => currentState = value; }

        void OnGUI()
        {
            if (currentState != null && Application.isEditor && currentStateDebug)
                GUILayout.Label("Current State: " + currentState.GetType().Name);
        }

        private void Awake()
        {
            // [แก้ไขป้องกัน Error] เช็คก่อนว่ามีกล้องเชื่อมต่ออยู่หรือไม่
            if (playerCamera != null)
            {
                cam = playerCamera.GetComponent<Camera>();
            }

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

        private void Update()
        {
            if (groundCheck != null)
            {
                bool currentlyGrounded = Physics.CheckSphere(groundCheck.position, 0.25f, groundMask, QueryTriggerInteraction.Ignore);

                if (currentlyGrounded)
                {
                    airTime += Time.deltaTime;
                    if (airTime > 0.2f)
                    {
                        isGrounded = true;
                    }
                }
                else
                {
                    isGrounded = false;
                    airTime = 0f;
                }
            }
            else { isGrounded = true; }

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

            // หมุนก้มเงยที่ระดับคอ (cameraParent) เพื่อรองรับระบบ TPS แบบก้มแล้วหมุนรอบตัว
            cameraParent.localRotation = Quaternion.Euler(xRotation, 0, currentTilt);

            // ปลดล็อกบรรทัด playerCamera.localRotation = Quaternion.identity ออกไปแล้ว
            // เพื่อให้ CameraSwitcher.cs สามารถทำแอนิเมชันตอนสลับร่างได้สมูทๆ!
        }

        public void UpdateVisuals()
        {
            if (cam == null) return; // ป้องกัน Error หากไม่มีกล้อง

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

        public bool HasCeiling() { return false; }
        public bool CheckLedge(out Vector3 climbPosition) { climbPosition = Vector3.zero; return false; }
        private void OnTriggerEnter(Collider other) { }
        private void OnTriggerExit(Collider other) { }
    }
}