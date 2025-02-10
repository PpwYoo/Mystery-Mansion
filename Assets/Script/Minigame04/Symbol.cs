using UnityEngine;
using UnityEngine.UI;

public class Symbol : MonoBehaviour
{
    public Image symbolImage; // รูปภาพของสัญลักษณ์
    public Sprite normalSprite;
    public Sprite highlightedSprite;

    private bool isHighlighted = false;

    private void Start()
    {
        // ตรวจสอบว่ามี Image Component หรือไม่
        if (symbolImage == null)
        {
            symbolImage = GetComponent<Image>();
        }

        UpdateSymbol();
    }

    public void SetSymbol(Sprite normal, Sprite highlighted, bool highlight)
    {
        normalSprite = normal;
        highlightedSprite = highlighted;
        isHighlighted = highlight;
        UpdateSymbol();
    }

    private void UpdateSymbol()
    {
        if (symbolImage != null) // ป้องกัน Null Reference
        {
            symbolImage.sprite = isHighlighted ? highlightedSprite : normalSprite;
        }
    }

    public void OnSelect()
    {
        GetComponent<Image>().color = Color.green; // เปลี่ยนสีให้รู้ว่าเลือกแล้ว

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SelectSymbol(this);
        }
    }

    public bool IsHighlighted()
    {
        return isHighlighted;
    }
}
