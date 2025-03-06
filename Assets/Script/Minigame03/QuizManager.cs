using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class QuestionData
{
    public Sprite image;
    public string question;
    public string[] answers;
    public int correctAnswerIndex;
}

public class QuizManager : MonoBehaviour
{
    public GameController gameController;

    [Header("UI Elements")]
    public Image questionImage;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;
    public TextMeshProUGUI categoryText;
    public GameObject correctOverlayPanel;
    public GameObject wrongOverlayPanel;

    [Header("Question Data")]
    public List<QuestionData> Informations;
    public List<QuestionData> Mathematics;
    public List<QuestionData> Country;
    public List<QuestionData> Analogy;
    public List<QuestionData> Logical_Reason;

    private List<QuestionData> currentQuestionList;
    private QuestionData currentQuestion;
    private string currentCategoryName;

    private int lastLoggedRound = -1; // ใช้เพื่อตรวจสอบว่ารอบเปลี่ยนหรือไม่

    void Start()
    {
        ResetQuestions();
        gameController = FindObjectOfType<GameController>();
        StartRound();
    }

    void Update()
    {
        if (gameController.currentRound != lastLoggedRound)
        {
            Debug.Log("Current Round: " + gameController.currentRound);
            lastLoggedRound = gameController.currentRound;
        }
    }

    void StartRound()
    {
        switch (gameController.currentRound)
        {
            case 1:
                currentQuestionList = new List<QuestionData>(Informations);
                currentCategoryName = "การตีความข้อมูล";
                break;
            case 2:
                currentQuestionList = new List<QuestionData>(Mathematics);
                currentCategoryName = "ปัญหาคณิตศาสตร์";
                break;
            case 3:
                currentQuestionList = new List<QuestionData>(Country);
                currentCategoryName = "ประเทศต่างๆ";
                break;
            case 4:
                currentQuestionList = new List<QuestionData>(Analogy);
                currentCategoryName = "อุปมาอุปไมย";
                break;
            case 5:
                currentQuestionList = new List<QuestionData>(Logical_Reason);
                currentCategoryName = "หาเหตุผลเชิงตรรกะ";
                break;
            default:
                Debug.Log("Invalid round!");
                return;
        }

        categoryText.text = "Category: " + currentCategoryName;

        if (currentQuestionList.Count > 0)
        {
            int randomIndex = Random.Range(0, currentQuestionList.Count);
            currentQuestion = currentQuestionList[randomIndex];
            currentQuestionList.RemoveAt(randomIndex);
            DisplayQuestion(currentQuestion);
        }
        else
        {
            Debug.Log("No more questions in this round!");
        }
    }

    void DisplayQuestion(QuestionData questionData)
    {
        questionImage.sprite = questionData.image;
        questionText.text = questionData.question;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < questionData.answers.Length)
            {
                answerButtons[i].gameObject.SetActive(true);
                answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = questionData.answers[i];
                answerButtons[i].onClick.RemoveAllListeners();
                int index = i;
                answerButtons[i].onClick.AddListener(() => CheckAnswer(index));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void CheckAnswer(int index)
    {
        if (index == currentQuestion.correctAnswerIndex)
        {
            Debug.Log("Correct Answer!");
            StartCoroutine(HandleCorrectAnswer());
        }
        else
        {
            StartCoroutine(HandleWrongAnswer());
        }
    }

    IEnumerator HandleCorrectAnswer()
{
    if (gameController.currentRound == gameController.totalRounds) 
    {
        // ถ้าเป็นคำถามสุดท้าย ให้ขึ้น SuccessPanel ทันที
        gameController.EndGame(true);
    }
    else
    {
        correctOverlayPanel.SetActive(true);
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(1f);
        correctOverlayPanel.SetActive(false);
        Time.timeScale = 1f;
        gameController.NextRound();
        StartRound();
    }
}

    IEnumerator HandleWrongAnswer()
    {
        wrongOverlayPanel.SetActive(true);
        yield return new WaitForSeconds(5f);
        wrongOverlayPanel.SetActive(false);
        DisplayQuestion(currentQuestion);
    }

    void ResetQuestions()
    {
        // คุณสามารถเพิ่มการรีเซตคำถามในหมวดต่างๆ ได้ที่นี่
    }
}
