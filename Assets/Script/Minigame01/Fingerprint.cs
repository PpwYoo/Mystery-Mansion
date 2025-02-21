using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class Fingerprint : MonoBehaviour
{
    // --- Data Class สำหรับคำถาม ---
    [System.Serializable]
    public class QuestionData
    {
        public Sprite questionImage;
        public Sprite[] choices;
        public Sprite[] selectedFrameSprites;
        public int correctAnswerIndex;
    }

    // --- Public Variables ---
    [Header("Question")]
    public QuestionData[] questions;

    [Header("Button")]
    public Button[] choiceButtons; public Button submitButton;

    [Header("GameObject")]
    public GameObject questionCanvas; public GameObject mapCanvas;
    public GameObject answerWrongPanel; public GameObject answerCorrectPanel;
    public GameObject mapGameOverPanel; public GameObject questionGameOverPanel; 
    public GameObject finishedPanel; public GameObject countdownPanel;

    [Header("Image")]
    public Image questionImage; public Image[] levelIcons;

    [Header("Text")]
    public TMP_Text mapTimerText; public TMP_Text questionTimerText; public TMP_Text countdownText;

    // --- Events ---
    public delegate void GameOverHandler();
    public event GameOverHandler OnGameOver;

    public delegate void MissionCompletedHandler();
    public event MissionCompletedHandler OnMissionCompleted;

    // --- Private Variables ---
    private float timer = 120f;
    private bool isTimerRunning = false;
    private QuestionData currentQuestion;
    private int selectedAnswerIndex = -1;
    private int currentLevel = 0;
    private int totalLevels;

    // --- Unity Lifecycle Methods ---
    void Start()
    {
        // ทำให้เปลี่ยน scene ของใครของมัน (ใครทำภารกิจเสร็จก่อนก็เปลี่ยนก่อน)
        PhotonNetwork.AutomaticallySyncScene = false;

        totalLevels = questions.Length;
        submitButton.interactable = false;
        submitButton.onClick.AddListener(SubmitAnswer);

        countdownPanel.SetActive(true);
        mapCanvas.SetActive(false);
        questionCanvas.SetActive(false);
        StartCoroutine(StartCountdown());
    }

    void Update()
    {
        UpdateTimer();
    }

    IEnumerator StartCountdown()
    {
        countdownPanel.SetActive(true);
        string[] countdownMessages = new string[] { "3", "2", "1", "Start!" };

        for (int i = 0; i < countdownMessages.Length; i++)
        {
            countdownText.text = countdownMessages[i];
            yield return new WaitForSeconds(1f);
        }

        countdownPanel.SetActive(false);

        ShowMap();
        StartTimer();
    }

    // --- Timer Methods ---
    public void StartTimer()
    {
        if (!isTimerRunning)
        {
            isTimerRunning = true;
            timer = 120f; // ตั้งเวลาเริ่มต้นที่ 120 วินาที
        }
    }

    private bool isGameOver = false; // Flag สำหรับตรวจสอบว่า GameOverPanel ถูกแสดงแล้วหรือไม่

    private void UpdateTimer()
    {
        if (isTimerRunning && !isGameOver)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                timer = 0;
                isTimerRunning = false;

                // เรียก Game Over Panel ทันทีเมื่อหมดเวลา
                isGameOver = true; // ป้องกันการเรียกซ้ำ
                if (mapCanvas.activeSelf)
                {
                    ShowGameOverPanel(false); // แสดง GameOverPanel สำหรับ MapCanvas
                }
                else if (questionCanvas.activeSelf)
                {
                    ShowGameOverPanel(true); // แสดง GameOverPanel สำหรับ QuestionCanvas
                }
            }

            // แปลงเวลาเป็นนาทีและวินาที
            int minutes = Mathf.FloorToInt(timer / 60);
            int seconds = Mathf.FloorToInt(timer % 60);

            // อัปเดตข้อความเวลาในรูปแบบ 00:00
            string timeFormatted = $"{minutes:D2}:{seconds:D2}";
            mapTimerText.text = $"{timeFormatted}";
            questionTimerText.text = $"{timeFormatted}";
        }
    }

    // --- UI Navigation Methods ---
    public void ShowMap()
    {
        mapCanvas.SetActive(true);
        questionCanvas.SetActive(false);
        answerWrongPanel.SetActive(false);
        answerCorrectPanel.SetActive(false);

        for (int i = 0; i < levelIcons.Length; i++)
        {
            Image levelIconImage = levelIcons[i];
            Button levelButton = levelIcons[i].GetComponent<Button>();

            if (i < currentLevel)
            {
                if (levelButton != null) levelButton.interactable = false;
            }
            else if (i == currentLevel)
            {
                levelIconImage.color = Color.white; // สีปกติ
                if (levelButton != null)
                {
                    levelButton.interactable = true;
                    levelButton.onClick.RemoveAllListeners();
                    int levelIndex = i;
                    levelButton.onClick.AddListener(() => ShowQuestion(levelIndex));
                }
            }
            else
            {
                levelIconImage.color = Color.gray; // สีจางสำหรับด่านที่ยังไม่ปลดล็อก
                if (levelButton != null) levelButton.interactable = false;
            }
        }
    }

    public void ShowQuestion(int questionIndex)
    {
        if (questionIndex >= totalLevels)
        {
            MissionCompleted();
            return;
        }

        mapCanvas.SetActive(false);
        questionCanvas.SetActive(true);

        currentQuestion = questions[questionIndex];
        questionImage.sprite = currentQuestion.questionImage;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < currentQuestion.choices.Length)
            {
                choiceButtons[i].image.sprite = currentQuestion.choices[i];
                choiceButtons[i].gameObject.SetActive(true);

                int choiceIndex = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choiceIndex));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }

        selectedAnswerIndex = -1;
        submitButton.interactable = false;
    }

    // --- Answer Selection Methods ---
    public void OnChoiceSelected(int index)
    {
        selectedAnswerIndex = index;
        Debug.Log($"Selected Answer: {index}");

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            choiceButtons[i].image.sprite = i == index 
                ? currentQuestion.selectedFrameSprites[i] 
                : currentQuestion.choices[i];
        }

        submitButton.interactable = true;
    }

    public void SubmitAnswer()
    {
        if (selectedAnswerIndex == -1) return;

        if (selectedAnswerIndex == currentQuestion.correctAnswerIndex)
        {
            Debug.Log("Correct Answer!");
        
            // ถ้าเป็นด่านสุดท้าย แสดง MissionCompleted
            if (currentLevel == totalLevels - 1)
            {
                MissionCompleted();
            }
            else
            {
                StartCoroutine(ShowCorrectAnswerPanel());
            }
        }
        else
        {
            Debug.Log("Wrong Answer! Try again.");
            StartCoroutine(ShowWrongAnswerPanel());
        }
    }

    // --- Panel Display Methods ---
    IEnumerator ShowWrongAnswerPanel()
    {
        answerWrongPanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        answerWrongPanel.SetActive(false);
        ShowQuestion(currentLevel); // กลับไปถามใหม่
    }

    IEnumerator ShowCorrectAnswerPanel()
    {
        answerCorrectPanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        answerCorrectPanel.SetActive(false);

        currentLevel++; // เพิ่มเลเวลหลังจากผ่านด่าน

        if (currentLevel < totalLevels)
        {
            ShowMap();
        }
        else
        {
            MissionCompleted(); // แสดง Finished Panel หากผ่านด่านสุดท้าย
        }
    }

    public void ShowGameOverPanel(bool isInQuestionCanvas)
    {
        OnGameOver?.Invoke(); // เรียก Event เมื่อเกมจบ

        // ปิด Panel ที่ไม่เกี่ยวข้อง
        answerWrongPanel.SetActive(false);
        answerCorrectPanel.SetActive(false);

        // บันทึกผลใน Photon
        string playerName = PhotonNetwork.NickName;
        string missionKey = "Mission_Fingerprint";
        string missionResult = "Fail";

        ExitGames.Client.Photon.Hashtable playerResults = new ExitGames.Client.Photon.Hashtable()
        {
            { $"{missionKey}_{playerName}", missionResult }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerResults);

        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable()
            {
                { "CurrentMission", missionKey }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }

        if (isInQuestionCanvas)
        {
            // เวลาหมดใน QuestionCanvas
            mapCanvas.SetActive(false);
            questionCanvas.SetActive(true); // ยังคงแสดง QuestionCanvas
            questionGameOverPanel.SetActive(true); // แสดง Panel เฉพาะของ QuestionCanvas
            Debug.Log("Game Over: Time ran out in QuestionCanvas");
        }
        else
        {
            // เวลาหมดใน MapCanvas
            questionCanvas.SetActive(false);
            mapCanvas.SetActive(true); // ยังคงแสดง MapCanvas
            mapGameOverPanel.SetActive(true); // แสดง Panel เฉพาะของ MapCanvas
            Debug.Log("Game Over: Time ran out in MapCanvas");
        }

        StartCoroutine(WaitAndChangeScene(2f));
    }

    public void MissionCompleted()
    {
        OnMissionCompleted?.Invoke();
        finishedPanel.SetActive(true);
        mapCanvas.SetActive(false);

        // บันทึกผลใน Photon
        string playerName = PhotonNetwork.NickName;
        string missionKey = "Mission_Fingerprint";
        string missionResult = "Complete";

        ExitGames.Client.Photon.Hashtable playerResults = new ExitGames.Client.Photon.Hashtable()
        {
            { $"{missionKey}_{playerName}", missionResult }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerResults);

        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable()
            {
                { "CurrentMission", missionKey }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }

        StartCoroutine(WaitAndChangeScene(2f));
    }

    IEnumerator WaitAndChangeScene(float delay)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.LoadLevel("WaitingScene");
    }
}
