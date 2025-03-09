using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameStartEffect : MonoBehaviour
{
    public Button startButton;
    public TextMeshProUGUI detectiveText; // ถ้าใช้ TextMeshPro
    public CanvasGroup startButtonGroup;
    public float fadeDuration = 1f; // ความเร็วในการทำให้ปุ่มจางหายไป

    private void Start()
    {
        startButton.onClick.AddListener(StartGame);
        detectiveText.gameObject.SetActive(false); // ซ่อนข้อความตอนเริ่มต้น
    }

    void StartGame()
    {
        // ทำให้ปุ่มค่อยๆ จางหายไป
        StartCoroutine(FadeOutButton());
    }

    IEnumerator FadeOutButton()
    {
        float timeElapsed = 0f;

        // ทำให้ปุ่มสามารถคลิกได้ก่อนที่มันจะเริ่มจาง
        startButton.interactable = true;

        while (timeElapsed < fadeDuration)
        {
            // ปรับ alpha โดยค่อยๆ ลดค่า
            startButtonGroup.alpha = Mathf.Lerp(1f, 0f, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;

            // รอให้กระบวนการดำเนินไปในแต่ละเฟรม
            yield return null;
        }

        // ปรับค่า alpha ให้เป็น 0
        startButtonGroup.alpha = 0f;
        startButton.interactable = false; // ปิดการใช้งานปุ่มเมื่อมันหายไป

        // หลังจากปุ่มหายไปแล้ว ให้เริ่มพิมพ์ข้อความ
        StartCoroutine(TypeMessage());
    }

    IEnumerator TypeMessage()
    {
        detectiveText.gameObject.SetActive(true); // แสดงข้อความ
        string message = "สวัสดี... คุณนักสืบ";
        detectiveText.text = ""; // รีเซ็ตข้อความ

        foreach (char letter in message.ToCharArray())
        {
            detectiveText.text += letter; // พิมพ์ตัวอักษรทีละตัว
            yield return new WaitForSeconds(0.1f); // ดีเลย์ระหว่างการพิมพ์
        }
    }
}
