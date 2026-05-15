using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenController : MonoBehaviour
{
    public static DeathScreenController instance;

    [Header("=== UI References ===")]
    public GameObject deathScreenUI;

    private bool isPlayerDead = false;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    // 👇👇 [ส่วนที่เพิ่มใหม่] ฟังก์ชันนี้จะทำงานอัตโนมัติทุกครั้งที่โหลดฉากใหม่เสร็จ 👇👇
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // รีเซ็ตสถานะให้กลับมามีชีวิต และซ่อนหน้าจอ You Died ทิ้งไป
        isPlayerDead = false;
        if (deathScreenUI != null) deathScreenUI.SetActive(false);
        Time.timeScale = 1f; // เผื่อมีการหยุดเวลาไว้ ให้กลับมาเดินปกติ
    }
    // 👆👆 ---------------------------------------------------------------- 👆👆

    private void Update()
    {
        // ถ้าตายแล้วและคลิกเมาส์ซ้าย
        if (isPlayerDead && Input.GetMouseButtonDown(0))
        {
            RestartGame();
        }
    }

    public void ShowDeathScreen()
    {
        if (deathScreenUI != null)
        {
            isPlayerDead = true;
            deathScreenUI.SetActive(true);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void RestartGame()
    {
        // ปิดจอดำก่อนโหลดฉาก
        isPlayerDead = false;
        if (deathScreenUI != null) deathScreenUI.SetActive(false);

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}