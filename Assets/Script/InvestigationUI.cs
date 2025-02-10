using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InvestigationUI : MonoBehaviour
{
    public GameObject panel; // Panel แสดงรายละเอียด
    public TMP_Text gameTitle; // ชื่อเกม
    // public Image minigameImage; // รูปภาพของมินิเกม
    public TMP_Text minigameDescription; // ข้อความรายละเอียด

    // ปิด Panel
    public void ClosePanel()
    {
        panel.SetActive(false);
    }

    void Start()
    {
        panel.SetActive(false);
    }

    public void OpenPanelGame1()
    {
        panel.SetActive(true);
        gameTitle.text = "Fingerprint";
    }

    public void OpenPanelGame2()
    {
        panel.SetActive(true);
        gameTitle.text = "SpotsHunt";
    }

    public void OpenPanelGame3()
    {
        panel.SetActive(true);
        gameTitle.text = "RandomQuiz";
    }

    public void OpenPanelGame4()
    {
        panel.SetActive(true);
        gameTitle.text = "RightSigns";
    }

    public void OpenPanelGame5()
    {
        panel.SetActive(true);
        gameTitle.text = "FindTheWay";
    }
}
