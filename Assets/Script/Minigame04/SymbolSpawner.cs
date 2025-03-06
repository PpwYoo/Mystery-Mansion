using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;

public class SymbolSpawner : MonoBehaviour
{
    [Header("Symebol Setting")]
    public GameObject symbolPrefab;
    public Sprite[] allSymbols;
    public Sprite[] framedSymbols;
    public TMP_Text instructionText;
    public TMP_Text timerText;
    public TMP_Text roundText;

    [Header("General Setting")]
    public GameObject selectionPanel;
    public TMP_Text selectionTimerText;
    public TMP_Text selectionRoundText;
    public TMP_Text selectionQText;
    public Transform selectionGrid;
    public Button confirmButton;

    [Header("Panel Overlay")]
    public GameObject greenOverlayPanel;
    public float greenOverlayDisplayTime = 1f;
    public GameObject redOverlayPanel;
    public GameObject finishedPanel;

    [Header("Countdown to Start")]
    public GameObject countdownCanvas;
    public TMP_Text countdownText;

    private List<GameObject> spawnedSymbols = new List<GameObject>();
    private List<Sprite> symbolsToRememberSprites = new List<Sprite>();
    private List<GameObject> selectedSymbols = new List<GameObject>();

    private int round = 1;
    private int maxRounds = 5;
    private float memorizeTime;
    private float selectionTime = 25f;

    // Start & Main Coroutine
    void Start()
    {
        // ทำให้เปลี่ยน scene ของใครของมัน (ใครทำภารกิจเสร็จก่อนก็เปลี่ยนก่อน)
        PhotonNetwork.AutomaticallySyncScene = false;

        confirmButton.onClick.AddListener(OnConfirmSelection);

        countdownCanvas.SetActive(true);
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        countdownCanvas.SetActive(true);
        string[] countdownMessages = new string[] { "3", "2", "1", "Start!" };

        for (int i = 0; i < countdownMessages.Length; i++)
        {
            countdownText.text = countdownMessages[i];
            yield return new WaitForSeconds(1f);
        }

        countdownCanvas.SetActive(false);
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        while (round <= maxRounds)
        {
            yield return StartCoroutine(StartRound());
        }

        ShowMissionCompleted();
    }

    // Round Management
    IEnumerator StartRound()
    {
        memorizeTime = 5 + round; // Reset Memorization Time

        // Show Memorization Panel
        timerText.gameObject.SetActive(true);
        instructionText.gameObject.SetActive(true);
        roundText.gameObject.SetActive(true);
        roundText.text = $"รอบที่ {round}/{maxRounds}";

        // Clear Previous Symbols
        foreach (var symbol in spawnedSymbols)
        {
            Destroy(symbol);
        }
        spawnedSymbols.Clear();

        // Generate New Symbols
        List<int> randomIndexes = Enumerable.Range(0, allSymbols.Length)
                                            .OrderBy(x => Random.value)
                                            .Take(9)
                                            .ToList();

        foreach (int index in randomIndexes)
        {
            GameObject newSymbol = Instantiate(symbolPrefab, transform);
            newSymbol.GetComponent<Image>().sprite = allSymbols[index];
            spawnedSymbols.Add(newSymbol);
            newSymbol.SetActive(true);
        }

        // Select Symbols to Remember
        int memorizeCount = round + 1;
        List<GameObject> symbolsToRemember = spawnedSymbols.OrderBy(x => Random.value)
                                                           .Take(memorizeCount)
                                                           .ToList();

        symbolsToRememberSprites.Clear();
        foreach (GameObject symbol in symbolsToRemember)
        {
            Image img = symbol.GetComponent<Image>();
            int index = System.Array.IndexOf(allSymbols, img.sprite);
            if (index != -1)
            {
                img.sprite = framedSymbols[index];
                symbolsToRememberSprites.Add(framedSymbols[index]);
            }
        }

        // Update Instruction Text to include number of symbols to memorize
        instructionText.text = $"สัญลักษณ์รอบนี้มี {memorizeCount} ตัว\nจดจำสัญลักษณ์ไว้ให้ดี!!";

        // Start Memorization Timer
        yield return StartCoroutine(CountdownTimer(memorizeTime));

        // Hide Memorization Panel and Show Selection
        roundText.gameObject.SetActive(false);
        instructionText.text = "";
        ShowSelectionPanel();
    }

    IEnumerator CountdownTimer(float time)
{
    float remainingTime = time;

    while (remainingTime > 0)
    {
        // แปลงเวลาที่เหลือเป็นนาทีและวินาที
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);

        // อัปเดตข้อความของตัวจับเวลาในรูปแบบ MM:SS
        timerText.text = $"{minutes:D2}:{seconds:D2}";

        yield return null; // อัปเดตทุกเฟรม
        remainingTime -= Time.deltaTime; // ลดเวลาลงตามเวลาจริง
    }

    timerText.text = "00:00"; // แสดง 00:00 เมื่อหมดเวลา
}

    void ShowSelectionPanel()
    {
        StopAllCoroutines(); // Stop All Coroutines

        // Hide Memorization Panel
        timerText.gameObject.SetActive(false);
        instructionText.gameObject.SetActive(false);
        foreach (var symbol in spawnedSymbols)
        {
            symbol.SetActive(false);
        }

        // Configure Selection Panel
        selectionRoundText.text = $"รอบที่ {round}/{maxRounds}";
        selectionPanel.SetActive(true);
        selectionGrid.gameObject.SetActive(true);

        // Clear Old Symbols in Selection Grid
        foreach (Transform child in selectionGrid)
        {
            Destroy(child.gameObject);
        }

        // Generate New Selection Symbols
        List<Sprite> selectionSprites = new List<Sprite>(symbolsToRememberSprites.Select(x => allSymbols[System.Array.IndexOf(framedSymbols, x)]));
        selectionSprites.AddRange(allSymbols.Except(selectionSprites).OrderBy(x => Random.value).Take(9 - selectionSprites.Count));
        selectionSprites = selectionSprites.OrderBy(x => Random.value).ToList();

        foreach (Sprite sprite in selectionSprites)
        {
            GameObject newSymbol = Instantiate(symbolPrefab, selectionGrid);
            newSymbol.GetComponent<Image>().sprite = sprite;

            Button button = newSymbol.GetComponent<Button>();
            button.onClick.AddListener(() => OnSymbolSelected(newSymbol, sprite));
        }

        // Start Selection Timer
        selectedSymbols.Clear();
        StartCoroutine(SelectionCountdown(selectionTime));
    }

    IEnumerator SelectionCountdown(float time)
{
    while (time > 0)
    {
        // แปลงเวลาที่เหลือเป็นนาทีและวินาที
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        // อัปเดตข้อความของตัวจับเวลาในรูปแบบ MM:SS
        selectionTimerText.text = $"{minutes:D2}:{seconds:D2}";

        // เช็คว่าผู้เล่นตอบถูกหรือยัง
        if (greenOverlayPanel.activeSelf)
        {
            yield break; // หยุด Coroutine ทันทีเมื่อคำตอบถูก
        }

        yield return null; // อัปเดตทุกเฟรม (แทน WaitForSeconds(1f))
        time -= Time.deltaTime; // ลดเวลาตามเวลาจริง
    }

    // หากเวลาหมดและยังไม่ตอบถูก
    if (time <= 0 && !greenOverlayPanel.activeSelf)
    {
        redOverlayPanel.SetActive(true);
        HandleGameOver();
    }
}

    void HandleGameOver()
    {
        redOverlayPanel.SetActive(true);
        StopAllCoroutines();

        SaveMissionResult(false);
    }

    IEnumerator ShowGreenOverlayAndProceed()
    {
        greenOverlayPanel.SetActive(true);
        yield return new WaitForSeconds(greenOverlayDisplayTime);
        greenOverlayPanel.SetActive(false);
        selectionPanel.SetActive(false);

        StartCoroutine(StartRound());
    }

    void ShowMissionCompleted()
    {
        finishedPanel.SetActive(true);
        StopAllCoroutines();
    }

    void OnConfirmSelection()
    {
        if (selectedSymbols.Count == symbolsToRememberSprites.Count)
        {
            CheckSelection(); // ตรวจสอบคำตอบ

            if (greenOverlayPanel.activeSelf) // หากคำตอบถูกต้อง
            {
                redOverlayPanel.SetActive(false); // ปิด redOverlayPanel เพื่อป้องกันซ้อน
            }
        }
        else
        {
            selectionQText.text = "เลือกให้ครบก่อนกดยืนยัน!!";
        }
    }

    void CheckSelection()
    {
        var selectedSprites = selectedSymbols.Select(x => x.GetComponent<Image>().sprite).ToList();

        if (selectedSprites.All(sprite => symbolsToRememberSprites.Contains(sprite)))
        {
            round++;

            // หยุดตัวจับเวลา
            StopCoroutine(nameof(SelectionCountdown));
            selectionTimerText.text = "";

            // แสดง GreenOverlayPanel และไปต่อรอบถัดไป
            greenOverlayPanel.SetActive(true);
            redOverlayPanel.SetActive(false);

            selectionQText.text = "เลือกสัญลักษณ์ที่ถูกต้อง";

            if (round > maxRounds)
            {
                SaveMissionResult(true);
                ShowMissionCompleted();
            }
            else
            {
                StartCoroutine(ShowGreenOverlayAndProceed());
            }
        }
        else
        {
            selectionQText.text = "เลือกผิด!! ลองอีกครั้ง";
            ShuffleAllSymbolsWithoutRevealingAnswers();
        }
    }

    void ShuffleAllSymbolsWithoutRevealingAnswers()
    {
        foreach (Transform child in selectionGrid)
        {
            Destroy(child.gameObject);
        }

        List<Sprite> newSelectionSprites = new List<Sprite>();
        List<Sprite> nonFramedMemorySymbols = symbolsToRememberSprites
            .Select(x => allSymbols[System.Array.IndexOf(framedSymbols, x)])
            .ToList();

        newSelectionSprites.AddRange(nonFramedMemorySymbols);
        List<Sprite> remainingNonFramedSymbols = allSymbols.Except(newSelectionSprites).ToList();
        newSelectionSprites.AddRange(remainingNonFramedSymbols.OrderBy(x => Random.value).Take(9 - newSelectionSprites.Count));
        newSelectionSprites = newSelectionSprites.OrderBy(x => Random.value).ToList();

        foreach (Sprite sprite in newSelectionSprites)
        {
            GameObject newSymbol = Instantiate(symbolPrefab, selectionGrid);
            newSymbol.GetComponent<Image>().sprite = sprite;

            Button button = newSymbol.GetComponent<Button>();
            button.onClick.AddListener(() => OnSymbolSelected(newSymbol, sprite));
        }

        selectedSymbols.Clear();
    }

    void OnSymbolSelected(GameObject symbol, Sprite sprite)
    {
        Image img = symbol.GetComponent<Image>();

        if (selectedSymbols.Contains(symbol))
        {
            selectedSymbols.Remove(symbol);
            int index = System.Array.IndexOf(framedSymbols, img.sprite);
            img.sprite = allSymbols[index];
        }
        else if (selectedSymbols.Count < symbolsToRememberSprites.Count)
        {
            selectedSymbols.Add(symbol);
            int index = System.Array.IndexOf(allSymbols, sprite);
            if (index != -1)
            {
                img.sprite = framedSymbols[index];
            }
        }
    }

    // เก็บผลภารกิจ
    void SaveMissionResult(bool isSuccess)
    {
        string playerName = PhotonNetwork.NickName;
        string missionKey = "Mission_RightSigns";
        string missionResult = isSuccess ? "Complete" : "Fail";

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

        Invoke("ChangeToWaitingScene", 2f);
    }

    void ChangeToWaitingScene()
    {
        PhotonNetwork.LoadLevel("WaitingScene");
    }
}
