using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // ต้องมีตัวนี้เพื่อใช้งาน Type "Button"

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("พิมพ์ชื่อ Scene ที่ต้องการโหลดที่นี่")]
    [SerializeField] private string sceneName;

    [Header("UI References")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        // 1. บังคับให้เม้าส์แสดงตัว
        Cursor.visible = true;

        // 2. ปลดล็อกเม้าส์จากการโดนล็อกไว้กลางจอ
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("Cursor has been restored by CursorFixer.");
    }

    private void Start()
    {
        // ตรวจสอบว่าได้ลากปุ่มมาใส่หรือยัง ถ้าใส่แล้วจะเชื่อมฟังก์ชันให้เองอัตโนมัติ
        if (playButton != null)
        {
            playButton.onClick.AddListener(PlayGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    public void PlayGame()
    {
        // ตรวจสอบก่อนว่าพิมพ์ชื่อซีนไว้หรือยัง
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("ยังไม่ได้พิมพ์ชื่อ Scene ใน Inspector นะครับ!");
        }
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}