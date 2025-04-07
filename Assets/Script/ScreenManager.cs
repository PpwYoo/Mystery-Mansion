using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    void Awake()
    {
#if UNITY_IOS
        Screen.orientation = ScreenOrientation.Portrait; // หรือ Landscape ตามต้องการ
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = true;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.fullScreen = true;

        // ปรับให้รองรับ Safe Area
        ApplySafeArea();
#endif
    }

    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        // ตัวอย่าง: log Safe Area
        Debug.Log("Safe Area: " + safeArea);

        // คุณสามารถปรับ UI Element ตาม safeArea ได้ที่นี่ เช่น ใช้ padding/margin
        // หรือส่ง safeArea นี้ให้ระบบจัดวาง UI ไปปรับให้
    }
}
