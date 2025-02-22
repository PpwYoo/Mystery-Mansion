using UnityEngine;
using TMPro;
using System.Collections;

public class GameController : MonoBehaviour
{
    public int totalRounds = 5; // Total rounds to be played
    public float timeLimit = 25f; // Time limit per round
    private float timer;
    public int currentRound = 1; // Current round (accessible from other scripts)

    public TextMeshProUGUI timerText; // Timer UI element
    public TextMeshProUGUI roundText; // Round UI element
    public GameObject successOverlay; // Overlay for mission success
    public GameObject gameOverOverlay; // Overlay for game over

    private bool isGameActive = true; // Flag to check if the game is active
    private int lastUpdatedRound = 0; // To prevent unnecessary round UI updates

    void Start()
    {
        if (timerText == null || roundText == null || successOverlay == null || gameOverOverlay == null)
        {
            Debug.LogError("UI elements or overlays are not assigned in the inspector!");
            isGameActive = false;
            return;
        }

        timer = timeLimit;
        UpdateRoundUI();
        UpdateTimerUI();
        successOverlay.SetActive(false);
        gameOverOverlay.SetActive(false);
    }

    void Update()
    {
        if (isGameActive && currentRound <= totalRounds)
        {
            timer -= Time.deltaTime;
            UpdateTimerUI();

            if (lastUpdatedRound != currentRound)
            {
                UpdateRoundUI();
                lastUpdatedRound = currentRound;
            }

            if (timer <= 0)
            {
                EndGame(false);
            }
        }
    }

    public void NextRound()
    {
        if (!isGameActive) return;

        if (currentRound < totalRounds)
        {
            currentRound++;
            timer = timeLimit;
            UpdateTimerUI();
        }
        else
        {
            EndGame(true);
        }
    }

    public void EndGame(bool isSuccess)
    {
        if (!isGameActive) return;

        isGameActive = false;
        timer = 0;
        UpdateTimerUI();

        if (isSuccess)
        {
            successOverlay.SetActive(true);
        }
        else
        {
            gameOverOverlay.SetActive(true);
        }
    }

    void UpdateRoundUI()
    {
        if (roundText != null)
        {
            roundText.text = currentRound + "/" + totalRounds;
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timer / 60);
            int seconds = Mathf.FloorToInt(timer % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void ResetGame()
    {
        if (!isGameActive) return;

        currentRound = 1;
        timer = timeLimit;
        UpdateRoundUI();
        UpdateTimerUI();

        if (successOverlay != null) successOverlay.SetActive(false);
        if (gameOverOverlay != null) gameOverOverlay.SetActive(false);

        isGameActive = true;
    }

    // ฟังก์ชันเรียกใช้เมื่อผู้เล่นตอบถูก
    public void CorrectAnswerSelected()
    {
        if (!isGameActive) return;

        if (currentRound == totalRounds)
        {
            EndGame(true); // แสดง successOverlay ทันทีเมื่อตอบถูกในรอบสุดท้าย
        }
        else
        {
            NextRound(); // ถ้ายังไม่ถึงรอบสุดท้าย ไปยังรอบถัดไป
        }
    }

    // ฟังก์ชันตรวจคำตอบ และเรียก CorrectAnswerSelected() ถ้าถูก
    public void OnAnswerSelected(bool isCorrect)
    {
        if (isCorrect)
        {
            CorrectAnswerSelected();
        }
        else
        {
            Debug.Log("Wrong answer! Try again.");
        }
    }
}
