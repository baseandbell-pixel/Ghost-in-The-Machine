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

        [Header("=== Slide Settings ===")]
        [Tooltip("ปุ่มสำหรับสไลด์ (ตั้งเป็น None เพื่อปิดระบบสไลด์)")]
        public KeyCode slideKey = KeyCode.None;
        [Tooltip("ระยะเวลาสไลด์")] public float slideDuration = 0.7f;
        [Tooltip("ความเร็วสไลด์")] public float slideSpeed = 6f;
        [Tooltip("ความกว้างหน้าจอ (FOV) ที่เพิ่มขึ้นตอนสไลด์")] public float slideFovBoost = 5f;

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
        public float fovChangeSpeed = 8f;
        public float bobAmount = 0.001f;
        public float bobSpeed = 10f;
        public float recoilReturnSpeed = 5f;

        [Header("=== Camera Heights ===")]
        public float standingCameraHeight = 1.75f;
        public float crouchingCameraHeight = 1f;
        public float crouchingCharacterControllerHeight = 1f;

        [Header("=== Animator Settings ===")]
        [Tooltip("ชื่อพารามิเตอร์ความเร็วใน Animator")] public string speedParameter = "Speed";
        [Tooltip("ชื่อพารามิเตอร์เช็คพื้นใน Animator")] public string groundedParameter = "isGrounded";
        [Tooltip("ชื่อพารามิเตอร์คำสั่งกระโดด (Trigger)")] public string jumpParameter = "Jump";

        [Header("=== Core References ===")]
        public Transform playerCamera;
        public Transform cameraParent;
        public Transform groundCheck;

        [Header("=== Debug & States ===")]
        public bool currentStateDebug = true;

        // ----------------------------------------------------
        // [ตัวแปรที่ซ่อนไว้ (Internal Variables)]
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
        [HideInInspector] public bool isSlideKeyPressed;

        // ตัวแปร Private ด้านระบบอนิเมชัน
        private Animator anim;

        // ตัวแปร Private ทั่วไป
        private PlayerBaseState currentState;
        private PlayerStateFactory states;
        private float xRotation = 0f;
        private float currentTilt;
        private float tiltVelocity;
        private float bobTimer;
        private float fovVelocity;
        private float originalCamY;
        private float landingMomentum;

        public PlayerBaseState CurrentState { get => currentState; set => currentState = value; }

        void OnGUI()
        {
            if (currentState != null && Application.isEditor && currentStateDebug)
                GUILayout.Label("Current State: " + currentState.GetType().Name);
        }

        private void Awake()
        {
            if (playerCamera != null)
            {
                cam = playerCamera.GetComponent<Camera>();
            }

            // ค้นหาคอมโพเนนต์ Animator จากโมเดลลูกอัตโนมัติ
            anim = GetComponentInChildren<Animator>();

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
            // ระบบเช็คการแตะพื้นแบบตอบสนองทันที (กระโดดสมูท)
            if (groundCheck != null)
            {
                isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, groundMask, QueryTriggerInteraction.Ignore);
            }
            else
            {
                isGrounded = true;
            }

            // ระบบเช็คปุ่มสไลด์ (ถ้าไม่ได้ตั้ง None ไว้)
            if (slideKey != KeyCode.None)
            {
                isSlideKeyPressed = Input.GetKeyDown(slideKey);
            }
            else
            {
                isSlideKeyPressed = false;
            }

            currentState.UpdateState();

            // เรียกใช้งานการอัปเดตค่าพารามิเตอร์อนิเมชัน
            UpdateAnimator();

            HandleRotation();
            UpdateVisuals();
        }

        private void UpdateAnimator()
        {
            if (anim == null || input == null) return;

            // ดึงค่าความเร็วจากฟิสิกส์แนวราบ (X, Z)
            Vector3 flatVelocity = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);
            float speed = flatVelocity.magnitude;

            // [Smart Fallback] ถ้าความเร็วฟิสิกส์เป็น 0 แต่ผู้เล่นยังกดปุ่มเดินอยู่ (แก้ปัญหาแอนิเมชันไม่เล่น)
            if (speed < 0.1f && input.moveInput.magnitude > 0.1f)
            {
                // ตรวจเช็คว่ากำลังวิ่งอยู่หรือไม่ โดยดูจากการขยายของช่วงมุมกล้อง (FOV)
                bool isSprinting = targetFov >= sprintFov;
                speed = isSprinting ? sprintSpeed : walkSpeed;
            }
            else if (input.moveInput.magnitude <= 0.1f)
            {
                // ถ้าปล่อยปุ่มเดินทั้งหมด ให้ความเร็วเป็น 0 ทันทีเพื่อกลับสู่ท่า Idle
                speed = 0f;
            }

            // ส่งค่าคำนวณความเร็วไปยัง Animator
            anim.SetFloat(speedParameter, speed);

            // ส่งสถานะสัมผัสพื้นไปยัง Animator
            anim.SetBool(groundedParameter, isGrounded);
        }

        // ฟังก์ชันภายนอกสำหรับสั่งให้ตัวละครเล่นแอนิเมชันกระโดด (สามารถเรียกใช้จาก Player State ต่างๆ ได้)
        public void TriggerJumpAnimation()
        {
            if (anim != null)
            {
                anim.SetTrigger(jumpParameter);
            }
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
            cameraParent.localRotation = Quaternion.Euler(xRotation, 0, currentTilt);
        }

        public void UpdateVisuals()
        {
            if (cam == null) return;

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