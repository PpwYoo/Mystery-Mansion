using System.Collections;
using System.Collections.Generic;
using System;
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
        public float roundTimer = 30f;
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
    public TMP_Text foundText;

    public GameObject successPanel;

    private int currentRound = 0;
    private int currentPuzzleSetIndex = 0;
    private float roundTimeRemaining;
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

    private bool isTargetedByVillain = false;

    [Header("BGM & SFX")]
    public AudioClip sceneBGM;
    public AudioClip missionStartSound;
    public AudioClip findSpotSound;
    public AudioClip endMissionSound;

    public AudioClip correctSound;

    private AudioManager audioManager;

    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();

        // ผู้เล่นที่ถูกคนร้ายเลือก
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("VillainTarget"))
        {
            string villainTarget = (string)PhotonNetwork.CurrentRoom.CustomProperties["VillainTarget"];
            if (villainTarget == PhotonNetwork.NickName)
            {
                isTargetedByVillain = true;
            }
        }
        else
        {
            Debug.Log("ไม่มีการเลือกผู้เล่นจากคนร้าย");
        }

        // ทำให้เปลี่ยน scene ของใครของมัน (ใครทำภารกิจเสร็จก่อนก็เปลี่ยนก่อน)
        PhotonNetwork.AutomaticallySyncScene = false;

        // สุ่มชุดโจทย์
        currentPuzzleSetIndex = UnityEngine.Random.Range(0, puzzleSets.Count);
        var selectedPuzzleSet = puzzleSets[currentPuzzleSetIndex];

        roundTimeRemaining = 0;
        foreach (var round in selectedPuzzleSet.rounds)
        {
            roundTimeRemaining += round.roundTimer;
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
            roundTimeRemaining -= Time.deltaTime;
            if (roundTimeRemaining <= 0)
            {
                roundTimeRemaining = 0;
                isTimerRunning = false;
                EndGame(false);
            }

            TimeSpan timeSpan = TimeSpan.FromSeconds(roundTimeRemaining);
            timerText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }
    }

    IEnumerator ShowCountdownAndStart()
    {
        string[] countdownMessages = new string[] { "3", "2", "1", "Start!" };
        audioManager.PlaySFX(missionStartSound);

        for (int i = 0; i < countdownMessages.Length; i++)
        {
            countdownText.text = countdownMessages[i];
            yield return new WaitForSeconds(1f);
        }

        countdownCanvas.SetActive(false);

        isGameActive = true;
        isTimerRunning = true;
        StartRound(0);

        if (AudioManager.instance != null)
        {
            AudioManager.instance.ChangeBGM(sceneBGM);
            AudioManager.instance.SetBGMVolume(1f);
        }
    }

    public void StartRound(int roundIndex)
    {
        ResetPreviousRound();

        var selectedPuzzleSet = puzzleSets[currentPuzzleSetIndex];
        currentRound = roundIndex;

        if (isTargetedByVillain && roundIndex == 4)
        {
            // ผู้เล่นที่ถูกคนร้ายเลือก ในรอบสุดท้ายรูปจะไม่ต่างกัน
            topImage.sprite = selectedPuzzleSet.rounds[roundIndex].topImage;
            bottomImage.sprite = selectedPuzzleSet.rounds[roundIndex].topImage;
            DisableDifferencePoints();
        }
        else
        {
            // กรณีปกติ
            topImage.sprite = selectedPuzzleSet.rounds[roundIndex].topImage;
            bottomImage.sprite = selectedPuzzleSet.rounds[roundIndex].bottomImage;
        }

        // ตั้งเวลาใหม่สำหรับรอบนี้
        roundTimeRemaining = selectedPuzzleSet.rounds[roundIndex].roundTimer;
        isTimerRunning = true;

        // รีเซ็ตจำนวนที่หาเจอ
        differencesFound = 0;

        // ซ่อน Success Panel ตอนเริ่มรอบใหม่
        successPanel.SetActive(false);

        // อัปเดตข้อความ foundText สำหรับรอบใหม่
        int totalDifferences = selectedPuzzleSet.rounds[roundIndex].differencePoints.Count;
        foundText.text = $"(หาเจอแล้ว {differencesFound}/{totalDifferences})";

        // แสดงปุ่มจุดต่าง
        foreach (var point in selectedPuzzleSet.rounds[roundIndex].differencePoints)
        {
            point.gameObject.SetActive(true);
        }

        roundText.text = $"รอบที่: {roundIndex + 1}/{selectedPuzzleSet.rounds.Count}";
    }

    private void DisableDifferencePoints()
    {
        var selectedPuzzleSet = puzzleSets[currentPuzzleSetIndex];
        foreach (var round in selectedPuzzleSet.rounds)
        {
            foreach (var point in round.differencePoints)
            {
                point.interactable = false;
            }
        }
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

        audioManager.PlaySFX(findSpotSound);
        differencesFound++;

        // อัปเดตข้อความ foundText เมื่อเจอจุดต่าง
        int totalDifferences = puzzleSets[currentPuzzleSetIndex].rounds[currentRound].differencePoints.Count;
        foundText.text = $"(หาเจอแล้ว {differencesFound}/{totalDifferences})";

        if (differencesFound >= totalDifferences)
        {
            // แสดง Success Panel แทนข้อความ
            Invoke(nameof(ShowSuccessPanel), 1f);

            var selectedPuzzleSet = puzzleSets[currentPuzzleSetIndex];

            // ถ้าหาเจอครบทุกจุด และเป็นรอบสุดท้าย (รอบที่ 5)
            if (currentRound + 1 >= selectedPuzzleSet.rounds.Count)
            {
                isTimerRunning = false;
                isGameActive = false;

                // แสดง resultCanvas ทันทีเมื่อจบเกม
                resultCanvas.SetActive(true);
                audioManager.PlaySFX(endMissionSound);
                resultText.text = "MISSION COMPLETED";

                // บันทึกผลลง Photon
                SaveMissionResult(true);

                Invoke("ChangeToWaitingScene", 2f);
            }
            else
            {
                // ถ้าไม่ใช่รอบสุดท้าย → เริ่มรอบถัดไปหลังจาก 2 วินาที
                Invoke(nameof(NextRound), 1f);
            }
        }
    }

    void ShowSuccessPanel()
    {
        successPanel.SetActive(true);
    }

    void NextRound()
    {
        var selectedPuzzleSet = puzzleSets[currentPuzzleSetIndex];
        audioManager.PlaySFX(correctSound);

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
        audioManager.PlaySFX(endMissionSound);
        resultText.text = isSuccess ? "MISSION COMPLETED" : "MISSION FAILED";

        // บันทึกผลลง Photon
        SaveMissionResult(isSuccess);

        Invoke("ChangeToWaitingScene", 2f);
    }

    void SaveMissionResult(bool isSuccess)
    {
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
