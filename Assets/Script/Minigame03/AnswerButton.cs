using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class AnswerButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Image buttonImage;
    public Sprite normalSprite;  // รูปปกติ
    public Sprite pressedSprite; // รูปเมื่อกด (สีแดง)
    public TextMeshProUGUI buttonText;

    private Color originalTextColor; // เก็บสีตัวอักษรเดิม

    void Start()
    {
        buttonImage.sprite = normalSprite; // ตั้งค่าเริ่มต้น
        ColorUtility.TryParseHtmlString("#7A1017", out originalTextColor); // กำหนดสีตัวอักษรเดิม
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        buttonImage.sprite = pressedSprite; // เปลี่ยนรูปเป็นสีแดง
        buttonText.color = Color.white; // เปลี่ยนตัวอักษรเป็นสีขาว
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        buttonImage.sprite = normalSprite; // เปลี่ยนกลับเป็นรูปปกติ
        buttonText.color = originalTextColor; // เปลี่ยนตัวอักษรกลับเป็นสีเดิม
    }
}
