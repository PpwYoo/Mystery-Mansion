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
    public List<QuestionData> fruitQuestionsOriginal;
    public List<QuestionData> countryQuestionsOriginal;
    public List<QuestionData> countingQuestionsOriginal;
    public List<QuestionData> importantPersonQuestionsOriginal;
    public List<QuestionData> analyticalThinkingQuestionsOriginal; // หมวดที่ 5 คิดวิเคราะห์

    private List<QuestionData> currentQuestionList;
    private QuestionData currentQuestion;
    private string currentCategoryName;

    void Start()
    {
        ResetQuestions();
        StartRound();
        gameController = FindObjectOfType<GameController>();
    }

    void Update()
    {
        Debug.Log("Current Round: " + gameController.currentRound);
    }

    void StartRound()
    {
        switch (gameController.currentRound)
        {
            case 1:
                currentQuestionList = new List<QuestionData>(fruitQuestionsOriginal);
                currentCategoryName = "ผลไม้";
                break;
            case 2:
                currentQuestionList = new List<QuestionData>(countryQuestionsOriginal);
                currentCategoryName = "ประเทศ";
                break;
            case 3:
                currentQuestionList = new List<QuestionData>(countingQuestionsOriginal);
                currentCategoryName = "นับจำนวน";
                break;
            case 4:
                currentQuestionList = new List<QuestionData>(importantPersonQuestionsOriginal);
                currentCategoryName = "บุคคลสำคัญ";
                break;
            case 5:
                currentQuestionList = new List<QuestionData>(analyticalThinkingQuestionsOriginal);
                currentCategoryName = "คิดวิเคราะห์";
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
        correctOverlayPanel.SetActive(true);
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(2f);
        correctOverlayPanel.SetActive(false);
        Time.timeScale = 1f;
        gameController.NextRound();
        StartRound();
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
    }
}
