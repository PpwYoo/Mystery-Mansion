using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Fingerprint : MonoBehaviour
{
    [System.Serializable]
    public class QuestionData
    {
        public Sprite questionImage;
        public Sprite[] choices;
        public Sprite[] selectedFrameSprites;
        public int correctAnswerIndex;
    }

    public QuestionData[] questions;
    public Button[] choiceButtons;
    public Button submitButton;
    public GameObject questionCanvas;
    public GameObject mapCanvas;
    public GameObject answerWrongPanel;
    public GameObject answerCorrectPanel;
    public Image questionImage;
    public TMP_Text mapTimerText; // ตัวแสดงเวลาใน MapCanvas
    public TMP_Text questionTimerText; // ตัวแสดงเวลาใน QuestionCanvas
    public Image[] levelIcons; // แว่นขยายที่ใช้แสดงด่าน

    private float timer = 120f;
    private bool isTimerRunning = false;
    private QuestionData currentQuestion;
    private int selectedAnswerIndex = -1;
    private int currentLevel = 0;
    private int totalLevels;

    // เพิ่ม Event สำหรับ Game Over และ Mission Completed
    public delegate void GameOverHandler();
    public event GameOverHandler OnGameOver;

    public delegate void MissionCompletedHandler();
    public event MissionCompletedHandler OnMissionCompleted;

    void Start()
    {
        totalLevels = questions.Length;
        ShowMap();
        StartTimer();

        submitButton.interactable = false;
        submitButton.onClick.AddListener(SubmitAnswer);

        // เริ่มต้นซ่อนแว่นขยายของด่านที่ยังไม่ผ่าน
        for (int i = 1; i < levelIcons.Length; i++) 
        {
            levelIcons[i].gameObject.SetActive(false); // ซ่อนด่านถัดไป
        }
    }

    void Update()
    {
        if (isTimerRunning)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0;
                isTimerRunning = false;
                Debug.Log("Time's up!");

                ShowGameOverPanel();  // แสดง Game Over Panel หากหมดเวลา
            }

            mapTimerText.text = $"TIME: {Mathf.CeilToInt(timer)}";
            questionTimerText.text = $"TIME: {Mathf.CeilToInt(timer)}";
        }
    }

    public void StartTimer()
    {
        if (!isTimerRunning)
        {
            isTimerRunning = true;
            timer = 120f;  // ตั้งเวลาเริ่มต้นที่ 180 วินาที
        }
    }

    public void ShowMap()
    {
        mapCanvas.SetActive(true);
        questionCanvas.SetActive(false);
        answerWrongPanel.SetActive(false);
        answerCorrectPanel.SetActive(false);

        // ซ่อนแว่นขยายของด่านที่ผ่านไปแล้ว และแสดงแว่นขยายของด่านถัดไป
        for (int i = 0; i < levelIcons.Length; i++) 
        {
            if (i < currentLevel) 
            {
                levelIcons[i].gameObject.SetActive(false);  // ซ่อนด่านที่ผ่านไปแล้ว
            }
            else if (i == currentLevel)
            {
                levelIcons[i].gameObject.SetActive(true);  // แสดงด่านปัจจุบัน
            }
        }
    }

    public void ShowQuestion(int questionIndex)
    {
        if (questionIndex >= totalLevels) 
        {
            MissionCompleted();  // เมื่อผ่านด่านทั้งหมด ให้เรียก MissionCompleted
            return;
        }

        // ซ่อนทุก Canvas ก่อนที่จะเปิดคำถามใหม่
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

    public void OnChoiceSelected(int index)
    {
        selectedAnswerIndex = index;
        Debug.Log($"Selected Answer: {index}");

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i == index)
            {
                choiceButtons[i].image.sprite = currentQuestion.selectedFrameSprites[i];
            }
            else
            {
                choiceButtons[i].image.sprite = currentQuestion.choices[i];
            }
        }

        submitButton.interactable = true;
    }

    public void SubmitAnswer()
    {
        if (selectedAnswerIndex == -1) return;

        if (selectedAnswerIndex == currentQuestion.correctAnswerIndex)
        {
            Debug.Log("Correct Answer!");
            StartCoroutine(ShowCorrectAnswerPanel());
        }
        else
        {
            Debug.Log("Wrong Answer! Try again.");
            StartCoroutine(ShowWrongAnswerPanel());
        }
    }

    IEnumerator ShowWrongAnswerPanel()
    {
        answerWrongPanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        answerWrongPanel.SetActive(false);
        ShowQuestion(currentLevel);  // กลับไปถามใหม่
    }

    IEnumerator ShowCorrectAnswerPanel()
    {
        answerCorrectPanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        answerCorrectPanel.SetActive(false);

        currentLevel++;

        // เมื่อผ่านด่านแล้ว แสดงด่านถัดไปและซ่อนด่านที่ผ่านไปแล้ว
        if (currentLevel < totalLevels)
        {
            ShowMap();  // กลับไปที่ MapCanvas เพื่อเล่นด่านถัดไป
        }
        else
        {
            MissionCompleted(); // เรียกใช้ event เมื่อเล่นครบทุกด่าน
        }
    }

    public void ShowGameOverPanel()
    {
        OnGameOver?.Invoke(); // เรียกใช้ event เมื่อเกมจบ
    }

    public void MissionCompleted()
    {
        OnMissionCompleted?.Invoke(); // เรียกใช้ event เมื่อสำเร็จ
    }
}
