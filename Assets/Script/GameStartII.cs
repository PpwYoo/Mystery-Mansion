using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class GameStartII : MonoBehaviourPunCallbacks
{
    [Header("General Setting")]
    public GameObject playerPrefab;
    public TMP_Text messageText;
    public TMP_Text timerText;
    public RectTransform[] playerPositions;
    private List<GameObject> currentPlayers = new List<GameObject>();

    [Header("Mission Setting")]
    public Image[] missionResultImages;
    public Sprite successSprite;
    public Sprite failSprite;
    Dictionary<string, int> missionPositions = new Dictionary<string, int>()
    { { "Mission_Fingerprint", 0 },{ "Mission_SpotDifference", 1 },{ "Mission_RandomQuiz", 2 },{ "Mission_RightSigns", 3 },{ "Mission_FindTheWay", 4 }};

    [Header("Discuss Time")]
    public float discussTimeLimit = 10f;
    private float timeRemaining;

    [Header("Employer Action")]
    public GameObject employerConfirmationPanel;
    public TMP_Text employerConfirmationText;
    public TMP_Text goodBadResult;

    private string employerselectedPlayer;
    public bool isEmployerSelectionActive = false;

    [Header("Villain Action")]
    public GameObject villainConfirmationPanel;
    public TMP_Text villainConfirmationText;

    private string villainselectedPlayer;
    public bool isVillainSelectionActive = false;

    [Header("Mission Fail System")]
    public GameObject susConfirmationPanel;
    public TMP_Text susConfirmationText;
    public TMP_Text susText;
    
    private string susSelectedPlayer;
    public bool issusSelectionActive = false;

    public float susTimeLimit = 10f;
    private float susTimeRemaining;
    private bool isSusTimeActive = false;

    private bool isSusVoteResultsFinished = false;
    private Dictionary<string, int> susVotes = new Dictionary<string, int>();
    private Dictionary<string, int> susVoteCounts = new Dictionary<string, int>();

    IEnumerator Start()
    {
        SetupPlayers();

        // แสดงผลภารกิจ
        foreach (string missionKey in missionPositions.Keys)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey($"Result_{missionKey}"))
            {
                bool isMissionSuccess = (bool)PhotonNetwork.CurrentRoom.CustomProperties[$"Result_{missionKey}"];
                missionResultImages[missionPositions[missionKey]].sprite = isMissionSuccess ? successSprite : failSprite;
            }
        }

        yield return new WaitUntil(() => PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.PlayerList.Length);

        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable()
        {
            { "EmployerActionDone", false },{ "VillainActionDone", false }
        });
        
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("LastMission", out object lastMissionKey))
        {
            Debug.Log($"Last Mission: {lastMissionKey}");
            yield return StartCoroutine(DelayMissionResult((string)lastMissionKey));

            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue($"Result_{lastMissionKey}", out object result) && result is bool isSuccess && !isSuccess)
            {
                yield return StartCoroutine(MissionFail());
            }
            else
            {
                StartDiscuss();
            }
        }
        else
        {
            messageText.text = "ยังไม่มีผลภารกิจ";
            yield return new WaitForSeconds(3f);
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Leader"))
        {
            string leader = (string)PhotonNetwork.CurrentRoom.CustomProperties["Leader"];
            SetLeader(leader);
        }
    }

    void SetupPlayers()
    {
        ClearExistingPlayers();
        Player[] players = PhotonNetwork.PlayerList;

        List<int> usedPositions = new List<int> { 0 };

        for (int i = 0; i < players.Length; i++)
        {
            int positionIndex = players[i].IsLocal ? 0 : -1;

            if (!players[i].IsLocal) // หาตำแหน่งว่างสำหรับผู้เล่นที่ไม่ใช่ LocalPlayer
            {
                for (int j = 1; j < playerPositions.Length; j++)
                {
                    if (!usedPositions.Contains(j)) // ถ้าตำแหน่งยังไม่ได้ใช้
                    {
                        positionIndex = j;
                        break;
                    }
                }
            }

            if (positionIndex != -1) // ตรวจสอบว่าพบตำแหน่งว่างหรือไม่
            {
                GameObject playerObject = Instantiate(playerPrefab);
                playerObject.transform.SetParent(playerPositions[positionIndex], false);

                currentPlayers.Add(playerObject);

                RectTransform rectTransform = playerObject.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Vector2.zero;
                }

                PlayerDisplay playerDisplay = playerObject.GetComponent<PlayerDisplay>();
                playerDisplay.SetPlayerInfo(players[i].NickName);

                // Get Player Role
                string role = GetPlayerRole(players[i]);
                playerDisplay.SetPlayerRole(role);

                if (!players[i].IsLocal)
                {
                    usedPositions.Add(positionIndex);
                }
            }
        }
    }

    void ClearExistingPlayers()
    {
        foreach (GameObject player in currentPlayers)
        {
            if (player != null)
            {
                Destroy(player);
            }
        }
        currentPlayers.Clear();
    }

    string GetPlayerRole(Player player)
    {
        if (player.CustomProperties.ContainsKey("Role"))
        {
            return (string)player.CustomProperties["Role"];
        }
        return "ไม่ทราบบทบาท";
    }

    void SetLeader(string leader)
    {
        foreach (GameObject playerObject in currentPlayers)
        {
            PlayerDisplay playerDisplay = playerObject.GetComponent<PlayerDisplay>();
            if (playerDisplay != null)
            {
                playerDisplay.ShowLeaderIcon(playerDisplay.playerName == leader);
            }
        }
    }

    void ShowMissionResult(string missionKey)
    {
        if (missionResultImages == null || missionResultImages.Length == 0) { return; }

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue($"Result_{missionKey}", out object result))
        {
            bool isMissionSuccess = (bool)result;
            messageText.text = isMissionSuccess ? "ภารกิจสำเร็จ" : "ภารกิจล้มเหลว";

            if (missionPositions.TryGetValue(missionKey, out int missionPosition))
            {
                if (missionPosition >= 0 && missionPosition < missionResultImages.Length)
                {
                    missionResultImages[missionPosition].sprite = isMissionSuccess ? successSprite : failSprite;
                }
            }
            else
            {
                Debug.LogWarning($"[GameStartII] Mission key '{missionKey}' not found in missionPositions dictionary");
            }
        }
        else
        {
            messageText.text = "ยังไม่มีผลภารกิจ";
        }
    }

    IEnumerator DelayMissionResult(string missionKey)
    {
        ShowMissionResult(missionKey);
        yield return new WaitForSeconds(3f);
    }

    // เช็คว่าตอนนี้มีภารกิจที่สำเร็จเท่าไหร่ ล้มเหลวเท่าไหร่
    void CheckMissionResult()
    {
        int successCount = 0;
        int failCount = 0;

        foreach (string missionKey in missionPositions.Keys)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey($"Result_{missionKey}"))
            {
                bool isMissionSuccess = (bool)PhotonNetwork.CurrentRoom.CustomProperties[$"Result_{missionKey}"];
                if (isMissionSuccess)
                {
                    successCount++;
                }
                else
                {
                    failCount++;
                }
            }
        }
        Debug.Log($"Mission Successes: {successCount}, Mission Fails: {failCount}");
    }

    void StartDiscuss()
    {
        timeRemaining = discussTimeLimit;
        messageText.text = "สนทนากัน";
        StartCoroutine(UpdateTimer());
    }

    IEnumerator UpdateTimer()
    {
        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            timerText.text = Utility.FormatTime(timeRemaining);
            
            yield return null;
        }
        EndDiscuss();
    }

    void EndDiscuss()
    {
        messageText.text = "หมดเวลาสนทนา";
        timerText.text = "";

        StartCoroutine(StartEmployerAndVillainActions());
    }

    IEnumerator StartEmployerAndVillainActions()
    {
        yield return new WaitForSeconds(3);
        messageText.text = "ระวังตัวด้วย คนร้ายจ้องจะเล่นคุณ!!";

        string localPlayerRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];

        if (localPlayerRole == "ผู้ว่าจ้าง")
        {
            messageText.text = "เลือกตรวจสอบผู้เล่น 1 คน";   
            PerformActionForEmployer(employerselectedPlayer);
        }
        else if (localPlayerRole == "คนร้าย")
        {
            messageText.text = "เลือกกลั่นแกล้งผู้เล่น 1 คน";
            PerformActionForVillain(villainselectedPlayer);
        }
        else
        {
            yield return new WaitForSeconds(3);
            messageText.text = "กรุณารอสักครู่...";
            yield return new WaitForSeconds(3);
            messageText.text = "ระวังตัวด้วย คนร้ายจ้องจะเล่นคุณ!!";
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        foreach (string missionKey in missionPositions.Keys)
        {
            if (propertiesThatChanged.ContainsKey($"Result_{missionKey}"))
            {
                ShowMissionResult(missionKey);
            }
        }

        CheckMissionResult();

        if (propertiesThatChanged.ContainsKey("EmployerActionDone") || propertiesThatChanged.ContainsKey("VillainActionDone"))
        {
            CheckIfBothActionsCompletedAndLoadScene();
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        // คนร้ายเลือกใคร
        if (targetPlayer == PhotonNetwork.LocalPlayer && changedProps.ContainsKey("VillainTarget"))
        {
            Debug.Log($"คนร้ายเลือกเป้าหมาย: {changedProps["VillainTarget"]}");
        }

        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        // การเก็บค่าการโหวตผู้ต้องสงสัย
        if (changedProps.ContainsKey("susVotes"))
        {
            string votedPlayer = (string)changedProps["susVotes"];

            if (!susVoteCounts.ContainsKey(votedPlayer))
            {
                susVoteCounts[votedPlayer] = 1;
            }
            else
            {
                susVoteCounts[votedPlayer]++;
            }

            Debug.Log($"ผู้เล่น {votedPlayer} ได้รับ {susVoteCounts[votedPlayer]} โหวต");
        }
    }

    // PlayerDisplay ไหนเป็นของผู้เล่นที่ถูกเลือก
    private PlayerDisplay FindPlayerDisplay(string playerName)
    {
        PlayerDisplay[] allPlayers = FindObjectsOfType<PlayerDisplay>();
        
        foreach (var player in allPlayers)
        {
            if (player.playerName == playerName)
            {
                return player;
            }
        }
        return null;
    }

    // -----------------------------------------------------------------------------------

    // ผู้ว่าจ้างตรวจสอบผู้เล่น
    public void PerformActionForEmployer(string playerName)
    {
        isEmployerSelectionActive = true;
        employerselectedPlayer = playerName;

        if (!string.IsNullOrEmpty(playerName))
        {
            ShowEmployerConfirmation(playerName);
        }
    }

    public void ShowEmployerConfirmation(string playerName)
    {
        employerselectedPlayer = playerName;
        employerConfirmationText.text = "ยืนยันการเลือกผู้เล่นนี้?";
        employerConfirmationPanel.SetActive(true);

        // ลด opacity ของผู้เล่นที่ถูกเลือก
        PlayerDisplay selectedPlayerDisplay = FindPlayerDisplay(playerName);
        if (selectedPlayerDisplay != null)
        {
            selectedPlayerDisplay.SetOpacity(0.5f);
        }
    }

    public void ConfirmEmployerSelection()
    {
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "EmployerActionDone", true } });

        PlayerDisplay selectedPlayerDisplay = FindPlayerDisplay(employerselectedPlayer);
        if (selectedPlayerDisplay != null)
        {
            string role = selectedPlayerDisplay.playerRole;

            if (role == "นักสืบ" || role == "ผู้ว่าจ้าง")
            {
                goodBadResult.text = "เป็นฝ่ายดี";
            }
            else if (role == "คนร้าย")
            {
                goodBadResult.text = "เป็นฝ่ายร้าย";
            }
            Invoke("ClearGoodBadResult", 3f);
            selectedPlayerDisplay.SetOpacity(1f);
        }

        employerConfirmationPanel.SetActive(false);
        isEmployerSelectionActive = false;
    }

    public void CancelEmployerSelection()
    {
        employerConfirmationPanel.SetActive(false);

        PlayerDisplay employerselectedPlayerDisplay = FindPlayerDisplay(employerselectedPlayer);
        if (employerselectedPlayerDisplay != null)
        {
            employerselectedPlayerDisplay.SetOpacity(1f);
        }
        
        employerselectedPlayer = null;
    }

    void ClearGoodBadResult()
    {
        goodBadResult.text = " ";
    }

    // -----------------------------------------------------------------------------------

    // คนร้ายเลือกผู้เล่น
    public void PerformActionForVillain(string playerName)
    {
        isVillainSelectionActive = true;
        villainselectedPlayer = playerName;

        if (!string.IsNullOrEmpty(playerName))
        {
            ShowVillainConfirmation(playerName);
        }
    }

    public void ShowVillainConfirmation(string playerName)
    {
        villainselectedPlayer = playerName;
        villainConfirmationText.text = "ยืนยันการเลือกผู้เล่นนี้?";
        villainConfirmationPanel.SetActive(true);

        // ลด opacity ของผู้เล่นที่ถูกเลือก
        PlayerDisplay selectedPlayerDisplay = FindPlayerDisplay(playerName);
        if (selectedPlayerDisplay != null)
        {
            selectedPlayerDisplay.SetOpacity(0.5f);
        }
    }

    public void ConfirmVillainSelection()
    {
        if (!string.IsNullOrEmpty(villainselectedPlayer))
        {
            SetVillainTarget(villainselectedPlayer);
        }

        villainConfirmationPanel.SetActive(false);
    }

    public void CancelVillainSelection()
    {
        villainConfirmationPanel.SetActive(false);

        PlayerDisplay villainselectedPlayerDisplay = FindPlayerDisplay(villainselectedPlayer);
        if (villainselectedPlayerDisplay != null)
        {
            villainselectedPlayerDisplay.SetOpacity(1f);
        }
        
        villainselectedPlayer = null;   
    }

    void SetVillainTarget(string playerName)
    {
        // บันทึกข้อมูลเป้าหมายของคนร้าย
        ExitGames.Client.Photon.Hashtable villainAction = new ExitGames.Client.Photon.Hashtable
        {
            { "VillainTarget", playerName }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(villainAction);

        FinishVillainSelection();
    }

    void FinishVillainSelection()
    {
        // อัปเดตสถานะของคนร้าย
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "VillainActionDone", true } });

        PlayerDisplay selectedPlayerDisplay = FindPlayerDisplay(villainselectedPlayer);
        if (selectedPlayerDisplay != null)
        {
            selectedPlayerDisplay.SetOpacity(1f);
        }

        isVillainSelectionActive = false;
    }

    // -----------------------------------------------------------------------------------

    void CheckIfBothActionsCompletedAndLoadScene()
    {
        int villainCount = 0;
        int employerCount = 0;
        
        // นับจำนวนผู้เล่นที่มีบทบาท "คนร้าย" และ "ผู้ว่าจ้าง"
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("Role", out object roleObj) && roleObj is string role)
            {
                if (role == "คนร้าย")
                    villainCount++;
                else if (role == "ผู้ว่าจ้าง")
                    employerCount++;
            }
        }

        bool allEmployerDone = employerCount > 0 && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("EmployerActionDone", out object employerDoneObj) && employerDoneObj is bool employerDone && employerDone;
        bool allVillainDone = villainCount > 0 && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("VillainActionDone", out object villainDoneObj) && villainDoneObj is bool villainDone && villainDone;

        if ((villainCount == 0 || allVillainDone) && (employerCount == 0 || allEmployerDone))
        {
            StartCoroutine(ShowMissionMessageAndLoadScene());
        }
    }

    IEnumerator ShowMissionMessageAndLoadScene()
    {
        messageText.text = "ได้เวลาทำภารกิจแล้ว";
        yield return new WaitForSeconds(3);
        PhotonNetwork.LoadLevel("MissionSelect");
    } 

    // -----------------------------------------------------------------------------------

    // ถ้าภารกิจล้มเหลว
    IEnumerator MissionFail()
    {
        issusSelectionActive = true;

        string[] missionFailMessages = new string[] { "ในเมื่อภารกิจล้มเหลว", "ในตอนนี้...", "คุณสงสัยใครอยู่ไหม", "เลือกผู้เล่นที่คุณสงสัย" };

        for (int i = 0; i < missionFailMessages.Length; i++)
        {
            messageText.text = missionFailMessages[i];
            yield return new WaitForSeconds(2f);
        }

        susTimeRemaining = susTimeLimit;
        isSusTimeActive = true;

        MissionFailSystem(susSelectedPlayer);
        StartCoroutine(SusTimer());
    }

    IEnumerator SusTimer()
    {
        while (susTimeRemaining > 0)
        {
            susTimeRemaining -= Time.deltaTime;
            timerText.text = Utility.FormatTime(susTimeRemaining);
            yield return null;
        }

        isSusTimeActive = false;
        issusSelectionActive = false;
        timerText.text = "";

        messageText.text = "หมดเวลาเลือกผู้ต้องสงสัย";
        yield return new WaitForSeconds(3f);

        SusVoteResults();
    }

    public void MissionFailSystem(string playerName)
    {
        if (!issusSelectionActive || !isSusTimeActive) return;
        susSelectedPlayer = playerName;

        if (!string.IsNullOrEmpty(playerName))
        {
            ShowSusConfirmation(playerName);
        }
    }

    public void ShowSusConfirmation(string playerName)
    {
        if (!isSusTimeActive) return;

        susSelectedPlayer = playerName;
        susConfirmationText.text = playerName + " น่าสงสัย?";
        susConfirmationPanel.SetActive(true);

        PlayerDisplay susSelectedPlayerDisplay = FindPlayerDisplay(playerName);
        if (susSelectedPlayerDisplay != null)
        {
            susSelectedPlayerDisplay.SetOpacity(0.5f);
        }
    }

    public void ConfirmSusSelection()
    {
        if (!isSusTimeActive || string.IsNullOrEmpty(susSelectedPlayer)) return;

        string localPlayerName = PhotonNetwork.LocalPlayer.NickName;

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable()
        {
            { "susVotes", susSelectedPlayer }
        });

        Debug.Log($"{localPlayerName} โหวตให้ {susSelectedPlayer}");

        PlayerDisplay susSelectedPlayerDisplay = FindPlayerDisplay(susSelectedPlayer);
        if (susSelectedPlayerDisplay != null)
        {
            susSelectedPlayerDisplay.SetOpacity(1f);
        }

        susConfirmationPanel.SetActive(false);
        issusSelectionActive = false;
    }

    public void CancelSusSelection()
    {
        if (!isSusTimeActive || string.IsNullOrEmpty(susSelectedPlayer)) return;
        susConfirmationPanel.SetActive(false);

        PlayerDisplay susSelectedPlayerDisplay = FindPlayerDisplay(susSelectedPlayer);
        if (susSelectedPlayerDisplay != null)
        {
            susSelectedPlayerDisplay.SetOpacity(1f);
        }
        
        susSelectedPlayer = null;   
    }

    public void SusVoteResults()
    {
        if (susVoteCounts.Count > 0)
        {
            isSusVoteResultsFinished = false;
            StartCoroutine(ShowSusVoteResultsOneByOne());
        }
        else
        {
            StartCoroutine(ShowNoSuspectsAndStartDiscuss());
        }
    }

    private IEnumerator ShowNoSuspectsAndStartDiscuss()
    {
        messageText.text = "";
        susText.text = "ไม่มีใครน่าสงสัย";

        yield return new WaitForSeconds(3f);
        
        susText.text = "";
        StartDiscuss();
    }

    private IEnumerator ShowSusVoteResultsOneByOne()
    {
        messageText.text = "";

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("LastMission", out object lastMissionKey))
        {
            string lastMission = (string)lastMissionKey;

            // เช็คคนที่โดนโหวต >= 2 คะแนน
            bool hasSuspects = false;
            foreach (var vote in susVoteCounts)
            {
                if (vote.Value >= 2)
                {
                    hasSuspects = true;
                    break;
                }
            }

            if (hasSuspects)
            {
                susText.text = "มีผู้ต้องสงสัย";
                yield return new WaitForSeconds(2f);

                susText.text = "มาดูผลภารกิจ\nของผู้ต้องสงสัยกัน";
                yield return new WaitForSeconds(2f);

                foreach (var vote in susVoteCounts)
                {
                    if (vote.Value >= 1)
                    {
                        string playerName = vote.Key;
                        string missionResultKey = $"{lastMission}_{playerName}";
                        string missionResult = "ไม่มีข้อมูล";

                        foreach (var player in PhotonNetwork.PlayerList)
                        {
                            if (player.NickName == playerName && player.CustomProperties.ContainsKey(missionResultKey))
                            {
                                missionResult = (string)player.CustomProperties[missionResultKey];
                                break;
                            }
                        }

                        susText.text = "ผลภารกิจของ " + playerName + "\nคือ " + missionResult;
                        yield return new WaitForSeconds(4f);
                    }
                }
            }
            else
            {
                susText.text = "ไม่มีใครน่าสงสัย";
                yield return new WaitForSeconds(3f);  
            }
        }

        isSusVoteResultsFinished = true;
        StartDiscussIfReady();
    }

    private void StartDiscussIfReady()
    {
        if (isSusVoteResultsFinished)
        { 
            susText.text = "";  
            StartDiscuss();
        }
    }
}
