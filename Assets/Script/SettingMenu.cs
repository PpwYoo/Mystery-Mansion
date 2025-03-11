using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingMenu : MonoBehaviour
{
    [Header("Settings Panel")]
    public GameObject SettingPanel;

    [Header("BGM Controls")]
    public Button bgmButton;
    public Image bgmImage;
    public Sprite bgmOnSprite;
    public Sprite bgmOffSprite;

    [Header("SFX Controls")]
    public Button sfxButton;
    public Image sfxImage;        
    public Sprite sfxOnSprite;  
    public Sprite sfxOffSprite;

    [Header("SFX Sounds")]
    public AudioClip startButtonClickSound;
    public AudioClip settingsButtonClickSound;
    public AudioClip closeButtonClickSound;

    private AudioManager audioManager;

    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();

        // ตั้งค่าการทำงานเมื่อกดปุ่ม BGM
        bgmButton.onClick.AddListener(() =>
        {
            audioManager.ToggleBGM(); // สลับสถานะเปิด/ปิด BGM
            UpdateButtonImage(); // อัปเดตรูปภาพของปุ่มให้ตรงกับสถานะ
        });

        sfxButton.onClick.AddListener(() =>
        {
            audioManager.ToggleSFX(); // สลับสถานะเปิด/ปิด SFX
            UpdateSFXButtonImage(); // อัปเดตรูปภาพของปุ่มให้ตรงกับสถานะ
        });

        SettingPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (audioManager == null)
        {
            audioManager = FindObjectOfType<AudioManager>();
        }

        UpdateButtonImage();
        UpdateSFXButtonImage();
    }

    // อัปเดตไอคอนของปุ่ม BGM ให้ตรงกับสถานะปัจจุบัน
    private void UpdateButtonImage()
    {
        bgmImage.sprite = audioManager.IsBGMMuted() ? bgmOffSprite : bgmOnSprite;
    }

    // อัปเดตไอคอนของปุ่ม SFX ให้ตรงกับสถานะปัจจุบัน
    private void UpdateSFXButtonImage()
    {
        sfxImage.sprite = audioManager.IsSFXMuted() ? sfxOffSprite : sfxOnSprite;
    }

    public void CloseSettings()
    {
        audioManager.PlaySFX(closeButtonClickSound);
        SettingPanel.SetActive(false);
    }

    // -------------------- SFX --------------------

    public void OnStartButtonClicked()
    {
        audioManager.PlaySFX(startButtonClickSound);
    }

    public void OnSettingsButtonClicked()
    {
        audioManager.PlaySFX(settingsButtonClickSound);

        SettingPanel.SetActive(true);
        UpdateButtonImage();
        UpdateSFXButtonImage();
    }
}
