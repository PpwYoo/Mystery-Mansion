using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI questionText;
    public GameObject phase1Panel;
    public GameObject phase2Panel;
    public Transform symbolGrid;
    public GameObject symbolPrefab;
    
    public List<Sprite> normalSprites;
    public List<Sprite> highlightedSprites;
    
    private float phase1Time = 10f;
    private float phase2Time = 20f;
    private int currentRound = 1;
    private int totalRounds = 5;
    
    private List<Symbol> correctSymbols = new List<Symbol>();
    private List<Symbol> selectedSymbols = new List<Symbol>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(Phase1());
    }

    IEnumerator Phase1()
    {
        phase1Panel.SetActive(true);
        phase2Panel.SetActive(false);
        GenerateSymbols();

        while (phase1Time > 0)
        {
            timerText.text = "Time: " + phase1Time.ToString("F0");
            yield return new WaitForSeconds(1f);
            phase1Time--;
        }

        StartCoroutine(Phase2());
    }

    void GenerateSymbols()
    {
        correctSymbols.Clear();
        foreach (Transform child in symbolGrid)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < 9; i++)
        {
            GameObject symbolObj = Instantiate(symbolPrefab, symbolGrid);
            Symbol symbolScript = symbolObj.GetComponent<Symbol>();

            bool isHighlighted = i < 3 + currentRound - 1;
            if (isHighlighted) correctSymbols.Add(symbolScript);

            symbolScript.SetSymbol(normalSprites[i], highlightedSprites[i], isHighlighted);
        }
    }

    IEnumerator Phase2()
    {
        phase1Panel.SetActive(false);
        phase2Panel.SetActive(true);
        questionText.text = "เลือกสัญลักษณ์ที่มีกรอบรอบที่แล้ว!";
        phase2Time = 20f;
        selectedSymbols.Clear();

        while (phase2Time > 0)
        {
            timerText.text = "Time: " + phase2Time.ToString("F0");
            yield return new WaitForSeconds(1f);
            phase2Time--;
        }

        GameOver();
    }

    public void SelectSymbol(Symbol symbol)
    {
        if (!selectedSymbols.Contains(symbol))
        {
            selectedSymbols.Add(symbol);
        }
    }

    public void CheckAnswer()
    {
        if (selectedSymbols.Count != correctSymbols.Count) return;

        foreach (Symbol symbol in selectedSymbols)
        {
            if (!correctSymbols.Contains(symbol))
            {
                questionText.text = "ไม่ถูกต้อง! ลองใหม่";
                return;
            }
        }

        NextRound();
    }

    void NextRound()
    {
        if (currentRound < totalRounds)
        {
            currentRound++;
            phase1Time = 10 - (currentRound - 1);
            StartCoroutine(Phase1());
        }
        else
        {
            questionText.text = "MISSION COMPLETED!";
        }
    }

    void GameOver()
    {
        questionText.text = "Game Over! ลองใหม่";
    }
}
