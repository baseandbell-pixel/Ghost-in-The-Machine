using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private Slider qualitySlider;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioMixer mixer;

    private void Start()
    {
        // โหลดค่าล่าสุดที่บันทึกไว้มาแสดงผลในหน้าจอตอนเริ่ม
        LoadSettingsToUI();
    }

    // ฟังก์ชันสำหรับ "คืนค่า" หรือ "โหลดค่าล่าสุด" มาใส่ UI
    public void LoadSettingsToUI()
    {
        qualitySlider.value = Settings.QualityLevel;
        volumeSlider.value = Settings.Volume;

        // อัปเดตเสียงและกราฟิกให้ตรงกับค่าที่บันทึกไว้ล่าสุด
        ApplyVisualAndAudio(Settings.QualityLevel, Settings.Volume);
    }

    // --- ฟังก์ชันสำหรับ Slider (On Value Changed) ---
    // ใช้เพื่อให้ผู้เล่นเห็นความเปลี่ยนแปลง/ได้ยินเสียงทันที แต่ยังไม่บันทึกลง Settings Class
    public void OnVolumeChanged(float value)
    {
        UpdateMixer(value);
    }

    public void OnQualityChanged(float value)
    {
        QualitySettings.SetQualityLevel((int)value);
    }

    // --- ฟังก์ชันหลัก ---

    // อัปเดต Mixer เฉยๆ
    private void UpdateMixer(float vol)
    {
        // ป้องกันค่า Log 0 โดยใช้ Mathf.Max
        float dB = Mathf.Log10(Mathf.Max(0.0001f, vol)) * 20;
        mixer.SetFloat("Master", dB);
    }

    // อัปเดตทั้งภาพและเสียง (ใช้ตอน Start หรือตอนคืนค่า)
    private void ApplyVisualAndAudio(int q, float v)
    {
        QualitySettings.SetQualityLevel(q);
        UpdateMixer(v);
    }

    // เรียกใช้เมื่อกดปุ่ม "Apply" เท่านั้น
    public void Apply()
    {
        // บันทึกค่าจาก UI ลงในตัวแปร Static ของเราจริงๆ
        Settings.QualityLevel = (int)qualitySlider.value;
        Settings.Volume = volumeSlider.value;

        Debug.Log("Saved: Quality " + Settings.QualityLevel + " | Volume " + Settings.Volume);
    }

    // เรียกใช้เมื่อกดปุ่ม "Back"
    public void Back()
    {
        // คืนค่า UI และความรู้สึก (เสียง/ภาพ) ให้กลับไปเป็นค่าที่บันทึกไว้ล่าสุด
        LoadSettingsToUI();

        // ปิดหน้าเมนู
        gameObject.SetActive(false);
    }
}