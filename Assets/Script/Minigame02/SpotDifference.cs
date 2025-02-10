using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    public List<RoundData> rounds;
    public Image topImage;
    public Image bottomImage;
    public TMP_Text timerText;
    public TMP_Text roundText;
    public TMP_Text messageText;
    public TMP_Text startText;

    private int currentRound = 0;
    private float timer;
    private bool isTimerRunning = false;
    private bool isGameActive = false;
    private int differencesFound = 0;

    private bool isRoundActive => isTimerRunning && isGameActive;

    void Start()
    {
        foreach (var round in rounds)
        {
            foreach (var point in round.differencePoints)
            {
                ConfigureButton(point);
            }
        }

        StartCoroutine(ShowCountdownAndStart());
    }

    void Update()
    {
        if (isRoundActive)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0;
                isTimerRunning = false;
                EndRound(false);
            }
            timerText.text = $"Time: {Mathf.CeilToInt(timer)}s";
        }
    }

    IEnumerator ShowCountdownAndStart()
    {
        isGameActive = false;
        startText.gameObject.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            startText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        startText.text = "Start!";
        yield return new WaitForSeconds(1f);

        startText.gameObject.SetActive(false);
        isGameActive = true;
        StartRound(0);
    }

    public void StartRound(int roundIndex)
    {
        ResetPreviousRound();

        currentRound = roundIndex;

        topImage.sprite = rounds[roundIndex].topImage;
        bottomImage.sprite = rounds[roundIndex].bottomImage;

        // แสดงปุ่มจุดต่าง
        foreach (var point in rounds[roundIndex].differencePoints)
        {
            point.gameObject.SetActive(true);
        }

        differencesFound = 0;
        timer = rounds[roundIndex].roundTimer;
        isTimerRunning = true;
        isGameActive = true;
        roundText.text = $"Round: {roundIndex + 1}/{rounds.Count}";
        messageText.text = "";
    }

    private void ResetPreviousRound()
    {
        if (currentRound < rounds.Count)
        {
            foreach (var point in rounds[currentRound].differencePoints)
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
        Debug.Log($"Round {currentRound + 1}: Difference found ({differencesFound}/{rounds[currentRound].differencePoints.Count})");

        if (differencesFound >= rounds[currentRound].differencePoints.Count)
        {
            EndRound(true);
        }
    }

    void EndRound(bool isSuccess)
    {
        isTimerRunning = false;
        isGameActive = false;

        if (isSuccess)
        {
            messageText.text = "Success!";
            Invoke(nameof(NextRound), 2f); // ไปยังรอบถัดไปหลังจาก 2 วินาที
        }
        else
        {
            messageText.text = "Failed!";
        }
    }

    void NextRound()
    {
        if (currentRound + 1 < rounds.Count)
        {
            StartRound(currentRound + 1);
        }
        else
        {
            messageText.text = "Mission Complete!";
            // สามารถเพิ่มการสรุปคะแนน หรือย้ายไป Scene อื่นได้
        }
    }

    public bool IsGameActive()
    {
        return isGameActive;
    }
}
