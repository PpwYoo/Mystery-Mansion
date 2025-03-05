using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.Linq;

public class Fingerprint : MonoBehaviour
{
    [System.Serializable]
    public class QuestionData
    {
        public Sprite questionImage;
        public Sprite HintFP;
        public Sprite[] choices;
        public Sprite[] selectedFrameSprites;
        public int correctAnswerIndex;
    }

    [Header("Question")]
    public QuestionData[] questions;

    [Header("Button")]
    public Button[] choiceButtons;
    public Button submitButton;

    [Header("GameObject")]
    public GameObject questionCanvas, mapCanvas, answerCorrectPanel, mapGameOverPanel, questionGameOverPanel, finishedPanel, countdownPanel;

    [Header("Image")]
    public Image questionImage;
    public Image hintFPImageUI;
    public Image[] levelIcons;

    [Header("Text")]
    public TMP_Text mapTimerText, questionTimerText, countdownText, answerWrongText;

    public delegate void GameOverHandler();
    public event GameOverHandler OnGameOver;

    public delegate void MissionCompletedHandler();
    public event MissionCompletedHandler OnMissionCompleted;

    private float timer = 120f;
    private bool isTimerRunning = false;
    private QuestionData currentQuestion;
    private int selectedAnswerIndex = -1;
    private int currentLevel = 0;
    private int totalLevels;
    private bool isGameOver = false;
    private List<QuestionData> shuffledQuestions = new List<QuestionData>();

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        totalLevels = questions.Length;
        submitButton.interactable = false;
        submitButton.onClick.AddListener(SubmitAnswer);

        // สุ่มลำดับโจทย์แต่คงลำดับตัวเลือก
        shuffledQuestions = questions.OrderBy(q => Random.value).ToList();

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
        string[] countdownMessages = { "3", "2", "1", "Start!" };

        foreach (var msg in countdownMessages)
        {
            countdownText.text = msg;
            yield return new WaitForSeconds(1f);
        }

        countdownPanel.SetActive(false);
        ShowMap();
        StartTimer();
    }

    public void StartTimer()
    {
        if (!isTimerRunning)
        {
            isTimerRunning = true;
            timer = 120f;
        }
    }

    private void UpdateTimer()
    {
        if (isTimerRunning && !isGameOver)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0;
                isTimerRunning = false;
                isGameOver = true;
                ShowGameOverPanel(mapCanvas.activeSelf);
            }

            int minutes = Mathf.FloorToInt(timer / 60);
            int seconds = Mathf.FloorToInt(timer % 60);
            string timeFormatted = $"{minutes:D2}:{seconds:D2}";

            mapTimerText.text = timeFormatted;
            questionTimerText.text = timeFormatted;
        }
    }

    public void ShowMap()
    {
        mapCanvas.SetActive(true);
        questionCanvas.SetActive(false);
        answerWrongText.gameObject.SetActive(false);
        answerCorrectPanel.SetActive(false);

        for (int i = 0; i < levelIcons.Length; i++)
        {
            Button levelButton = levelIcons[i].GetComponent<Button>();
            levelIcons[i].color = i == currentLevel ? Color.white : Color.gray;
            if (levelButton != null)
            {
                levelButton.interactable = i == currentLevel;
                levelButton.onClick.RemoveAllListeners();
                int levelIndex = i;
                levelButton.onClick.AddListener(() => ShowQuestion(levelIndex));
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

        currentQuestion = shuffledQuestions[questionIndex];
        questionImage.sprite = currentQuestion.questionImage;

        // แสดงภาพโจทย์ลายนิ้วมือ (HintFP)
        if (currentQuestion.HintFP != null)
        {
            hintFPImageUI.sprite = currentQuestion.HintFP;
            hintFPImageUI.gameObject.SetActive(true);
        }
        else
        {
            hintFPImageUI.gameObject.SetActive(false);
        }

        // แสดงตัวเลือกตามลำดับเดิม
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

    public void SubmitAnswer()
    {
        if (selectedAnswerIndex == -1) return;

        if (selectedAnswerIndex == currentQuestion.correctAnswerIndex)
        {
            Debug.Log("Correct Answer!");

            if (currentLevel == 4)
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
            StartCoroutine(ShowWrongAnswerMessage());
        }
    }

    private bool canSelect = true; // ควบคุมการเลือกคำตอบและกดปุ่ม

    IEnumerator ShowWrongAnswerMessage()
    {
        canSelect = false; // ปิดการเลือกคำตอบและปุ่มยืนยัน
        submitButton.interactable = false; // ปิดปุ่มยืนยัน
        answerWrongText.gameObject.SetActive(true);
        answerWrongText.text = "ผิด!! เลือกใหม่อีกครั้ง";

        yield return new WaitForSeconds(3f);

        answerWrongText.gameObject.SetActive(false);
        canSelect = true; // เปิดให้เลือกคำตอบได้อีกครั้ง
        submitButton.interactable = true; // เปิดปุ่มยืนยัน

        // แสดงคำถามเดิม ให้เลือกใหม่จนกว่าจะตอบถูก
        ShowQuestion(currentLevel);
    }

    // ตรวจสอบ canSelect ก่อนให้เลือกคำตอบ
    public void OnChoiceSelected(int index)
    {
        if (!canSelect) return; // ถ้ากดไม่ได้ ให้ return ออกไป

        selectedAnswerIndex = index;
        Debug.Log($"Selected Answer: {index}");

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            choiceButtons[i].image.sprite = i == index ? currentQuestion.selectedFrameSprites[i] : currentQuestion.choices[i];
        }
        submitButton.interactable = true;
    }

    // ตรวจสอบ canSelect ก่อนให้กดปุ่มยืนยัน
    public void OnConfirmButtonPressed()
    {
        if (!canSelect) return; // ถ้ากดไม่ได้ ให้ return ออกไป
        SubmitAnswer();
    }

    IEnumerator ShowCorrectAnswerPanel()
    {
        answerCorrectPanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        answerCorrectPanel.SetActive(false);

        currentLevel++;

        if (currentLevel <= 4) 
        {
            ShowMap();
        }
        else 
        {
            MissionCompleted();
        }
    }

    public void ShowGameOverPanel(bool isInQuestionCanvas)
    {
        OnGameOver?.Invoke();
        answerWrongText.gameObject.SetActive(false);
        answerCorrectPanel.SetActive(false);

        string playerName = PhotonNetwork.NickName;
        string missionKey = "Mission_Fingerprint";
        string missionResult = "Fail";

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { $"{missionKey}_{playerName}", missionResult } });

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "CurrentMission", missionKey } });
        }

        if (isInQuestionCanvas)
        {
            mapCanvas.SetActive(false);
            questionCanvas.SetActive(true);
            questionGameOverPanel.SetActive(true);
        }
        else
        {
            questionCanvas.SetActive(false);
            mapCanvas.SetActive(true);
            mapGameOverPanel.SetActive(true);
        }

        StartCoroutine(WaitAndChangeScene(2f));
    }

    public void MissionCompleted()
    {
        OnMissionCompleted?.Invoke();
        questionCanvas.SetActive(true);
        mapCanvas.SetActive(false);
        answerCorrectPanel.SetActive(false);
        questionGameOverPanel.SetActive(false);
        mapGameOverPanel.SetActive(false);
        finishedPanel.SetActive(true);

        string playerName = PhotonNetwork.NickName;
        string missionKey = "Mission_Fingerprint";
        string missionResult = "Complete";

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { $"{missionKey}_{playerName}", missionResult } });

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "CurrentMission", missionKey } });
        }

        StartCoroutine(WaitAndChangeScene(2f));
    }

    IEnumerator WaitAndChangeScene(float delay)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.LoadLevel("WaitingScene");
    }
}
