using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // โหลดการตั้งค่าเสียงที่บันทึกไว้
        bgmSource.mute = PlayerPrefs.GetInt("BGM_Mute", 0) == 1;
        sfxSource.mute = PlayerPrefs.GetInt("SFX_Mute", 0) == 1;
    }

    // เปลี่ยน BGM
    public void ChangeBGM(AudioClip newBGM)
    {
        if (bgmSource.clip == newBGM) return;

        bgmSource.clip = newBGM;
        bgmSource.Play();
    }

    // สลับสถานะเปิด/ปิดเสียง BGM
    public void ToggleBGM()
    {
        bgmSource.mute = !bgmSource.mute;
        PlayerPrefs.SetInt("BGM_Mute", bgmSource.mute ? 1 : 0);
        PlayerPrefs.Save();
    }

    // สลับสถานะเปิด/ปิดเสียง SFX
    public void ToggleSFX()
    {
        sfxSource.mute = !sfxSource.mute;
        PlayerPrefs.SetInt("SFX_Mute", sfxSource.mute ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ตรวจสอบสถานะว่าปัจจุบัน BGM ถูกปิดเสียงหรือไม่
    public bool IsBGMMuted()
    {
        return bgmSource.mute;
    }

    // ตรวจสอบสถานะว่า SFX ถูกปิดเสียงหรือไม่
    public bool IsSFXMuted()
    {
        return sfxSource.mute;
    }

    // เล่นเสียง SFX ที่กำหนด
    public void PlaySFX(AudioClip clip)
    {
        if (!IsSFXMuted())
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // ปรับ volume BGM บางฉาก
    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null)
        {
            bgmSource.volume = Mathf.Clamp(volume, 0f, 1f);
        }
    }
}
