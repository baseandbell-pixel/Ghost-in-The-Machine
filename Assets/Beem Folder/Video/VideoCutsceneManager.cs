using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoCutsceneManager : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    [Tooltip("ชื่อ Scene ที่จะให้ไปต่อหลังจากวิดีโอเล่นจบ")]
    public string nextSceneName = "MAP2";

    [Tooltip("ปุ่มสำหรับกดข้ามวิดีโอ (เผื่อผู้เล่นไม่อยากดู)")]
    public KeyCode skipKey = KeyCode.Space;

    void Start()
    {
        // ดึง Component VideoPlayer ที่อยู่ใน GameObject เดียวกันมาใช้งาน
        videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
        {
            // สมัครรับ Event: เมื่อวิดีโอเล่นมาถึงเฟรมสุดท้าย ให้เรียกฟังก์ชัน OnVideoEnd
            videoPlayer.loopPointReached += OnVideoEnd;
        }
        else
        {
            Debug.LogError("หา Video Player ไม่เจอครับ!");
        }
    }

    void Update()
    {
        // ระบบกดข้ามคัตซีน
        if (Input.GetKeyDown(skipKey))
        {
            Debug.Log("ผู้เล่นกดข้ามคัตซีน...");
            LoadNextScene();
        }
    }

    // ฟังก์ชันนี้จะถูกเรียกอัตโนมัติเมื่อวิดีโอเล่นจบ
    void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("วิดีโอเล่นจบแล้ว กำลังโหลดฉากต่อไป...");
        LoadNextScene();
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}