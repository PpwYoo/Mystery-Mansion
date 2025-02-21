using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class SpotDifference : MonoBehaviour
{
    [System.Serializable]
    public class RoundData
    {
        public Sprite topImage;
        public Sprite bottomImage;
        public List<Button> differencePoints;
        public float roundTimer = 40f;
    }

    [System.Serializable]
    public class PuzzleSet
    {
        public List<RoundData> rounds;
    }

    public List<PuzzleSet> puzzleSets;
    public Image topImage;
    public Image bottomImage;
    public TMP_Text timerText;
    public TMP_Text roundText;
    public TMP_Text messageText;
    public TMP_Text startText;

    private int currentRound = 0;
    private int currentPuzzleSetIndex = 0;
    private float totalTime;
    private bool isTimerRunning = false;
    private bool isGameActive = false;
    private int differencesFound = 0;

    private bool isRoundActive => isTimerRunning && isGameActive;

    [Header("Countdown to Start")]
    public GameObject countdownCanvas;
    public TMP_Text countdownText;

    [Header("Mission Result")]
    public GameObject resultCanvas;
    public TMP_Text resultText;

    void Start()
    {
        // ทำให้เปลี่ยน scene ของใครของมัน (ใครทำภารกิจเสร็จก่อนก็เปลี่ยนก่อน)
        PhotonNetwork.AutomaticallySyncScene = false;

        // สุ่มชุดโจทย์
        currentPuzzleSetIndex = Random.Range(0, puzzleSets.Count);
        var selectedPuzzleSet = puzzleSets[currentPuzzleSetIndex];

        totalTime = 0;
        foreach (var round in selectedPuzzleSet.rounds)
        {
            totalTime += round.roundTimer;
        }

        // ซ่อนปุ่มที่เคยเลือกไว้
        foreach (var round in selectedPuzzleSet.rounds)
        {
            foreach (var point in round.differencePoints)
            {
                ConfigureButton(point);
            }
        }

        resultCanvas.SetActive(false);
        countdownCanvas.SetActive(true);
        StartCoroutine(ShowCountdownAndStart());
    }

    void Update()
    {
        if (isRoundActive)
        {
            totalTime -= Time.deltaTime;
            if (totalTime <= 0)
            {
                totalTime = 0;
                isTimerRunning = false;
                EndGame(false);
            }
            timerText.text = $"Time: {Mathf.CeilToInt(totalTime)}s";
        }
    }

    IEnumerator ShowCountdownAndStart()
    {
        string[] countdownMessages = new string[] { "3", "2", "1", "Start!" };

        for (int i = 0; i < countdownMessages.Length; i++)
        {
            countdownText.text = countdownMessages[i];
            yield return new WaitForSeconds(1f);
        }

        countdownCanvas.SetActive(false);

        isGameActive = true;
        isTimerRunning = true;
        StartRound(0);
    }

    public void StartRound(int roundIndex)
    {
        ResetPreviousRound();
        
        var selectedPuzzleSet = puzzleSets[currentPuzzleSetIndex];
        currentRound = roundIndex;

        topImage.sprite = selectedPuzzleSet.rounds[roundIndex].topImage;
        bottomImage.sprite = selectedPuzzleSet.rounds[roundIndex].bottomImage;

        // แสดงปุ่มจุดต่าง
        foreach (var point in selectedPuzzleSet.rounds[roundIndex].differencePoints)
        {
            point.gameObject.SetActive(true);
        }

        differencesFound = 0;
        roundText.text = $"Round: {roundIndex + 1}/{selectedPuzzleSet.rounds.Count}";
        messageText.text = "";
    }

    private void ResetPreviousRound()
    {
        var selectedPuzzleSet = puzzleSets[currentPuzzleSetIndex];
        if (currentRound < selectedPuzzleSet.rounds.Count)
        {
            foreach (var point in selectedPuzzleSet.rounds[currentRound].differencePoints)
            {
                ConfigureButton(point);
            }
        }
    }

    private void ConfigureButton(Button button)
    {
        var differencePoint = button.GetComponent<DifferencePoint>();
        differencePoint?.ResetPoint();
        button.gameObject.SetActive(false);
    }

    public void OnDifferenceFound(Button point)
    {
        if (!isGameActive) return;

        differencesFound++;
        Debug.Log($"Round {currentRound + 1}: Difference found ({differencesFound}/{puzzleSets[currentPuzzleSetIndex].rounds[currentRound].differencePoints.Count})");

        if (differencesFound >= puzzleSets[currentPuzzleSetIndex].rounds[currentRound].differencePoints.Count)
        {
            messageText.text = "Success!";

            var selectedPuzzleSet = puzzleSets[currentPuzzleSetIndex];
            if (currentRound + 1 >= selectedPuzzleSet.rounds.Count)
            {
                isTimerRunning = false;
                isGameActive = false;
            }

            Invoke(nameof(NextRound), 2f);
        }
    }

    void NextRound()
    {
        var selectedPuzzleSet = puzzleSets[currentPuzzleSetIndex];

        if (currentRound + 1 < selectedPuzzleSet.rounds.Count)
        {
            StartRound(currentRound + 1);
        }
        else
        {
            EndGame(true);
        }
    }

    void EndGame(bool isSuccess)
    { 
        resultCanvas.SetActive(true);  
        resultText.text = isSuccess ? "Mission Complete!" : "Mission Fail!";
        
        string playerName = PhotonNetwork.NickName;
        string missionKey = "Mission_SpotDifference";
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

    public bool IsGameActive()
    {
        return isGameActive;
    }
}
