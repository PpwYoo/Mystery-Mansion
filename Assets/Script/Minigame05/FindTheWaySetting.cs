using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class FindTheWaySetting : MonoBehaviour
{
    public TMP_Text timerText, roundText, messageText;
    public Button leftButton, rightButton, upButton, downButton;
    public GameObject moveArrowCanvas;

    [Header("Player & Goal")]
    public RectTransform playerUI; public RectTransform goalImage; public RectTransform gridPanelRectTransform;

    private float totalGameTime = 180f;
    private int currentRound = 0;
    private bool isGameActive = false;
    private Vector2 currentGridPosition;

    private Vector2 goalPosition;
    private Vector2[] trapPositions;

    private int gridSize = 5;

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
        
        currentGridPosition = new Vector2(0,0);
        SetupButtons();

        moveArrowCanvas.SetActive(false);
        resultCanvas.SetActive(false);
        countdownCanvas.SetActive(true);
        StartCoroutine(CountdownToStart());

        UpdatePlayerUIPosition();

        goalImage.gameObject.SetActive(false);
        playerUI.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isGameActive)
        {
            totalGameTime -= Time.deltaTime;
            if (totalGameTime <= 0)
            {
                totalGameTime = 0;
                EndGame(false);
            }
            UpdateTimerUI();
        }
    }

    void SetupButtons()
    {
        leftButton.onClick.AddListener(() => MovePlayer(Vector2.left));
        rightButton.onClick.AddListener(() => MovePlayer(Vector2.right));
        upButton.onClick.AddListener(() => MovePlayer(Vector2.up));
        downButton.onClick.AddListener(() => MovePlayer(Vector2.down));
    }

    void MovePlayer(Vector2 direction)
    {
        if (!isGameActive) return;
        currentGridPosition += new Vector2(direction.x, -direction.y);

        currentGridPosition.x = Mathf.Clamp(currentGridPosition.x, 0, gridSize - 1);
        currentGridPosition.y = Mathf.Clamp(currentGridPosition.y, 0, gridSize - 1);

        UpdatePlayerUIPosition();

        // เหยียบโดนกับดัก
        if (IsTrap(currentGridPosition))
        {
            ResetPlayerPosition();
            return;
        }

        // ถึงเส้นชัย
        if (IsGoal(currentGridPosition))
        {
            StartRound();
        }
    }

    void UpdatePlayerUIPosition()
    {
        Vector2 cellSize = new Vector2(gridPanelRectTransform.rect.width / gridSize, gridPanelRectTransform.rect.height / gridSize);
        Vector2 newAnchoredPosition = new Vector2(currentGridPosition.x * cellSize.x - gridPanelRectTransform.rect.width / 2 + cellSize.x / 2,-currentGridPosition.y * cellSize.y + gridPanelRectTransform.rect.height / 2 - cellSize.y / 2);

        playerUI.anchoredPosition = newAnchoredPosition;
    }

    IEnumerator CountdownToStart()
    {
        string[] countdownMessages = new string[] { "3", "2", "1", "Start!" };

        for (int i = 0; i < countdownMessages.Length; i++)
        {
            countdownText.text = countdownMessages[i];
            yield return new WaitForSeconds(1f);
        }

        countdownCanvas.SetActive(false);
        moveArrowCanvas.SetActive(true);

        StartRound();
    }

    void StartRound()
    {
        playerUI.gameObject.SetActive(true);
        if (currentRound >= 5)
        {
            EndGame(true);
            return;
        }

        currentRound++;
        roundText.text = $"{currentRound}/5";

        int numberOfTraps = 5 + currentRound;

        // ตำแหน่งเส้นชัย
        Vector2 goal = new Vector2(gridSize - 1, gridSize - 1);

        // สุ่มตำแหน่งกับดัก
        Vector2[] traps = new Vector2[numberOfTraps];
        bool trapsValid = false;

        while (!trapsValid)
        {
            traps = new Vector2[numberOfTraps];

            for (int i = 0; i < traps.Length; i++)
            {
                traps[i] = new Vector2(UnityEngine.Random.Range(0, gridSize), UnityEngine.Random.Range(0, gridSize));

                while (traps[i] == goal || traps[i] == new Vector2(0, 0) || IsDuplicateTrap(traps, traps[i], i))
                {
                    traps[i] = new Vector2(UnityEngine.Random.Range(0, gridSize), UnityEngine.Random.Range(0, gridSize));
                }
            }
            trapsValid = CheckIfPathIsValid(traps, goal);
        }

        SetTrapPositions(traps);
        SetGoalPosition(goal);

        Invoke(nameof(ActivateRound), 0f);
    }

    bool CheckIfPathIsValid(Vector2[] traps, Vector2 goal)
    {
        bool[,] grid = new bool[gridSize, gridSize];
        
        foreach (var trap in traps)
        {
            grid[(int)trap.x, (int)trap.y] = true;
        }

        // ใช้ BFS ค้นหาเส้นทาง
        Queue<Vector2> queue = new Queue<Vector2>();
        bool[,] visited = new bool[gridSize, gridSize];

        queue.Enqueue(new Vector2(0, 0));
        visited[0, 0] = true;

        Vector2[] directions = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };

        while (queue.Count > 0)
        {
            Vector2 current = queue.Dequeue();
            
            if (current == goal)
            {
                return true; // มีเส้นทางไปถึงเป้าหมาย
            }
            
            foreach (var dir in directions)
            {
                Vector2 newPos = current + dir;
                
                if (newPos.x >= 0 && newPos.x < gridSize && newPos.y >= 0 && newPos.y < gridSize && !grid[(int)newPos.x, (int)newPos.y] && !visited[(int)newPos.x, (int)newPos.y])
                {
                    queue.Enqueue(newPos);
                    visited[(int)newPos.x, (int)newPos.y] = true;
                }
            }
        }
        return false; // ไม่มีเส้นทางไปถึงเป้าหมาย
    }

    bool IsDuplicateTrap(Vector2[] traps, Vector2 newTrap, int currentIndex)
    {
        for (int i = 0; i < currentIndex; i++)
        {
            if (traps[i] == newTrap)
            {
                return true;
            }
        }
        return false;
    }

    void SetGoalPosition(Vector2 goal)
    {
        goalPosition = goal;
        UpdateGoalImagePosition();
    }

    void UpdateGoalImagePosition()
    {
        Vector2 cellSize = new Vector2(gridPanelRectTransform.rect.width / gridSize, gridPanelRectTransform.rect.height / gridSize);
        Vector2 newAnchoredPosition = new Vector2(goalPosition.x * cellSize.x - gridPanelRectTransform.rect.width / 2 + cellSize.x / 2,-goalPosition.y * cellSize.y + gridPanelRectTransform.rect.height / 2 - cellSize.y / 2);

        goalImage.anchoredPosition = newAnchoredPosition;
        goalImage.gameObject.SetActive(true);
    }

    void SetTrapPositions(Vector2[] traps)
    {
        trapPositions = traps;
    }

    bool IsTrap(Vector2 position)
    {
        foreach (Vector2 trap in trapPositions)
        {
            if (position == trap)
            {
                return true; // ผู้เล่นเหยียบกับดัก
            }
        }
        return false; // ไม่มีการเหยียบกับดัก
    }

    bool IsGoal(Vector2 position)
    {
        return position == goalPosition;
    }

    void ActivateRound()
    {
        messageText.text = "Round " + currentRound;
        Invoke(nameof(ClearMessageText), 1f);

        currentGridPosition = new Vector2(0, 0);
        UpdatePlayerUIPosition();
        isGameActive = true;
    }

    void ResetPlayerPosition()
    {
        currentGridPosition = new Vector2(0,0);
        UpdatePlayerUIPosition();

        messageText.text = "Trap!!";
        Invoke(nameof(ClearMessageText), 1f);
    }

    void ClearMessageText()
    {
        messageText.text = "";
    }

    void EndGame(bool isSuccess)
    {
        isGameActive = false;

        playerUI.gameObject.SetActive(false);
        goalImage.gameObject.SetActive(false);
        
        resultCanvas.SetActive(true);
        resultText.text = isSuccess ? "Mission Complete!" : "Mission Fail!";

        string playerName = PhotonNetwork.NickName;
        string missionKey = "Mission_FindTheWay";
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

    void UpdateTimerUI()
{
    TimeSpan timeSpan = TimeSpan.FromSeconds(totalGameTime);
    timerText.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
}

    void ChangeToWaitingScene()
    {
        PhotonNetwork.LoadLevel("WaitingScene");
    }
}
