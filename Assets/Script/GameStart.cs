using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class GameStart : MonoBehaviourPunCallbacks
{
    [Header("General Setting")]
    public GameObject playerPrefab;
    public TMP_Text messageText;
    public TMP_Text randomRoleText;
    public GameObject lineShowPanel;
    public RectTransform[] playerPositions;
    private List<GameObject> currentPlayers = new List<GameObject>();

    [Header("Game Instruction")]
    public GameObject instructionPanel;
    public Image instructionImage;
    public Button nextButton;
    public Button backButton;
    public Button okButton;

    public List<Sprite> instructionSprites;
    private int currentIndex = 0;

    public TMP_Text timerText;
    public float[] countdownTimes = { 10f, 60f, 30f };

    // [Header("Vote Result")]
    private Dictionary<string, string> votes = new Dictionary<string, string>();
    private Dictionary<string, int> voteCounts = new Dictionary<string, int>();
    private bool isVotingTimeOver = false;

    [Header("Vote Result")]
    public TMP_Text captainText;
    public GameObject voteResultsPanel;

    public GameObject confirmationPanel;
    public TMP_Text confirmationText;
    private string selectedPlayer;
    private bool isVotingEnabled = false;

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

    [Header("SFX")]
    public AudioClip assignRoleSound;
    public AudioClip endCaptainSound;
    public AudioClip messageSound;
    public AudioClip warningSound;
    public AudioClip announceSound;

    public AudioClip confirmSound;
    public AudioClip cancelSound;

    private AudioManager audioManager;

    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();

        SetupPlayers();
        StartCoroutine(AssignRolesWithDelay());

        nextButton.onClick.AddListener(() => ChangeInstructionIndex(1));
        backButton.onClick.AddListener(() => ChangeInstructionIndex(-1));
        okButton.onClick.AddListener(CloseInstructions);

        instructionPanel.SetActive(false);

        confirmationPanel.SetActive(false);
        voteResultsPanel.SetActive(false);

        employerConfirmationPanel.SetActive(false);
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

    IEnumerator AssignRolesWithDelay()
    {
        lineShowPanel.SetActive(true);

        yield return new WaitForSeconds(1);
        audioManager.PlaySFX(assignRoleSound);
        randomRoleText.text = "ณ ที่แห่งนี้";

        yield return new WaitForSeconds(3);
        randomRoleText.text = "บทบาทของคุณคือ";
        yield return new WaitForSeconds(3);

        AssignRoles();
    }

    void AssignRoles()
    {
        List<string> rolePool = GenerateRolePool();
        Player[] players = PhotonNetwork.PlayerList;

        audioManager.PlaySFX(announceSound);

        for (int i = 0; i < players.Length; i++)
        {
            string assignedRole = rolePool[i];
            players[i].SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Role", assignedRole } });

            Debug.Log($"Player {players[i].NickName} has been assigned the role: {assignedRole}");

            if (i < currentPlayers.Count)
            {
                currentPlayers[i].GetComponent<PlayerDisplay>()?.SetPlayerRole(assignedRole);
            }
            if (players[i].IsLocal)
            {
                StartCoroutine(DisplayRoleForDuration(assignedRole, 3));
            }

            StartCoroutine(ShowMissionLeaderMessage());
        }

        List<string> GenerateRolePool()
        {
            // List<string> roles = new List<string> { "คนร้าย", "คนร้าย", "ผู้ว่าจ้าง" };

            List<string> roles = new List<string> { "คนร้าย", "ผู้ว่าจ้าง" };
            int numDetectives = PhotonNetwork.PlayerList.Length - roles.Count;
            for (int i = 0; i < numDetectives; i++) roles.Add("นักสืบ");

            roles.Shuffle();
            return roles;
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        // การเก็บค่าบทบาทของผู้เล่น
        if (changedProps.ContainsKey("Role"))
        {
            string role = (string)changedProps["Role"];

            foreach (GameObject playerObject in currentPlayers)
            {
                PlayerDisplay playerDisplay = playerObject.GetComponent<PlayerDisplay>();
                if (playerDisplay != null && playerDisplay.playerName == targetPlayer.NickName)
                {
                    if (targetPlayer.IsLocal)
                    {
                        playerDisplay.SetPlayerRole(role);
                        StartCoroutine(DisplayRoleForDuration(role, 3));
                    }
                }
            }
        }

        // การเก็บค่าการโหวตหัวหน้าภารกิจ
        if (changedProps.ContainsKey("Votes"))
        {
            string votedPlayer = (string)changedProps["Votes"];
            if (voteCounts.ContainsKey(votedPlayer))
            {
                voteCounts[votedPlayer]++;
            }
            else
            {
                voteCounts[votedPlayer] = 1;
            }
            Debug.Log($"ผู้เล่น {votedPlayer} ได้รับ {voteCounts[votedPlayer]} โหวต");
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("Leader"))
        {
            string leader = (string)PhotonNetwork.CurrentRoom.CustomProperties["Leader"];
            Debug.Log($"หัวหน้าภารกิจคือ: {leader}");

            captainText.text = leader;
        }

        if (propertiesThatChanged.ContainsKey("EmployerActionDone") || propertiesThatChanged.ContainsKey("VillainActionDone"))
        {
            CheckIfBothActionsCompletedAndLoadScene();
        }

        // การกระทำของคนร้าย
        if (propertiesThatChanged.ContainsKey("VillainTarget"))
        {
            string villainTarget = (string)PhotonNetwork.CurrentRoom.CustomProperties["VillainTarget"];
            Debug.Log($"เป้าหมายของคนร้ายคือ: {villainTarget}");
        }
    }
    
    // ข้อความแสดงบทบาท
    private IEnumerator DisplayRoleForDuration(string role, float duration)
    {
        randomRoleText.text = role;
        yield return new WaitForSeconds(duration);
        lineShowPanel.SetActive(false);
        randomRoleText.text = "";
    }

    // ข้อความแสดงให้เลือกหัวหน้าภารกิจ
    private IEnumerator ShowMissionLeaderMessage()
    {
        yield return new WaitForSeconds(5);
        audioManager.PlaySFX(messageSound);
        messageText.text = "ถึงเวลาเลือกหัวหน้าภารกิจแล้ว";

        yield return new WaitForSeconds(3);
        instructionPanel.SetActive(true);
        UpdateInstruction();

        yield return StartCoroutine(CountdownTime1());
    }
    
    // Instruction
    void ChangeInstructionIndex(int direction)
    {
        currentIndex = Mathf.Clamp(currentIndex + direction, 0, instructionSprites.Count - 1);
        UpdateInstruction();
    }

    void UpdateInstruction()
    {
        instructionImage.sprite = instructionSprites[currentIndex];

        backButton.gameObject.SetActive(currentIndex > 0);
        nextButton.gameObject.SetActive(currentIndex < instructionSprites.Count - 1);
        okButton.gameObject.SetActive(currentIndex == instructionSprites.Count - 1);
    }

    void CloseInstructions()
    {
        instructionPanel.SetActive(false);
        messageText.text = "สนทนากันเพื่อหาหัวหน้าภารกิจ";

        isVotingEnabled = true;
    }

    // ตัวจับเวลาโหวตหัวหน้า
    IEnumerator CountdownTime1()
    {
        isVotingTimeOver = false;

        float timer = countdownTimes[0];
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            timerText.text = Utility.FormatTime(timer);
            yield return null;
        }
        timerText.text = Utility.FormatTime(0);
        isVotingTimeOver = true;

        audioManager.PlaySFX(endCaptainSound);
        messageText.text = "หมดเวลา";
        timerText.text = "00:00";

        yield return new WaitForSeconds(3);
        timerText.text = "";
        messageText.text = "";

        instructionPanel.SetActive(false);
        CalculateVote();
    }

    // คำนวนผลโหวตหัวหน้า
    void CalculateVote()
    {
        int highestVote = 0;
        List<string> captainsList = new List<string>();

        foreach (var vote in voteCounts)
        {
            if (vote.Value > highestVote)
            {
                highestVote = vote.Value;
                captainsList.Clear();
                captainsList.Add(vote.Key);
            }
            else if (vote.Value == highestVote)
            {
                captainsList.Add(vote.Key);
            }
        }

        if (captainsList.Count > 0)
        {
            string selectedLeader = captainsList[Random.Range(0, captainsList.Count)]; // ถ้าคะแนนเท่ากัน ให้สุ่มหัวหน้า
            SetLeader(selectedLeader);
        }

        instructionPanel.SetActive(false);
        voteResultsPanel.SetActive(true);
        audioManager.PlaySFX(announceSound);
        
        StartCoroutine(CleanupAfterVoting());
    }

    // ติดป้ายให้หัวหน้า
    void SetLeader(string leader)
    {
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable { { "Leader", leader } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        captainText.text = leader;

        foreach (GameObject playerObject in currentPlayers)
        {
            PlayerDisplay playerDisplay = playerObject.GetComponent<PlayerDisplay>();
            playerDisplay?.ShowLeaderIcon(playerDisplay.playerName == leader);
        }
    }

    // หลังจากเลือกหัวหน้าเสร็จ
    IEnumerator CleanupAfterVoting()
    {
        yield return new WaitForSeconds(6);
        voteResultsPanel.SetActive(false);

        yield return new WaitForSeconds(3);
        messageText.text = "ระวังตัว!! คนร้ายจ้องจะเล่นคุณ";

        // ตรวจสอบบทบาทของผู้เล่น
        string localPlayerRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
        if (localPlayerRole == "ผู้ว่าจ้าง")
        {
            audioManager.PlaySFX(messageSound);
            messageText.text = "เลือกตรวจสอบผู้เล่น 1 คน";
            PerformActionForEmployer(employerselectedPlayer);
        }
        else if (localPlayerRole == "คนร้าย")
        {
            audioManager.PlaySFX(messageSound);
            messageText.text = "เลือกกลั่นแกล้งผู้เล่น 1 คน";
            PerformActionForVillain(villainselectedPlayer);
        }
        else
        {
            yield return new WaitForSeconds(3);
            audioManager.PlaySFX(messageSound);
            messageText.text = "กรุณารอสักครู่...";

            yield return new WaitForSeconds(3);
            audioManager.PlaySFX(messageSound);
            messageText.text = "ระวังตัว!! คนร้ายจ้องจะเล่นคุณ";
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

    // เลือกหัวหน้าภารกิจ
    public void OnCaptainSelected(string playerName)
    {
        if (!isVotingEnabled || isEmployerSelectionActive || isVillainSelectionActive)
        {
            Debug.Log("ยังไม่สามารถเลือกหัวหน้าได้");
            return;
        }
        
        if (isVotingTimeOver)
        {
            Debug.Log("ไม่สามารถโหวตได้ เพราะหมดเวลาแล้ว");
            return;
        }
        ShowVoteConfirmation(playerName);
    }

    // ยืนยันการเลือกหัวหน้าไหม
public void ShowVoteConfirmation(string playerName)
{
    string localPlayer = PhotonNetwork.LocalPlayer.NickName;

    if (playerName == localPlayer)
    {
        audioManager.PlaySFX(warningSound);
        StopCoroutine(ResetMessageText()); // หยุด Coroutine เดิมถ้ามี
        messageText.text = "คุณไม่สามารถโหวตตัวเองได้";  
        confirmationPanel.SetActive(false);

        StartCoroutine(ResetMessageText()); // รอ 3 วินาทีแล้วกลับไปแสดงข้อความปกติ
        return;
    }

    selectedPlayer = playerName;

    audioManager.PlaySFX(warningSound);
    confirmationText.text = "แน่ใจใช่ไหม?";
    confirmationPanel.SetActive(true);

    // ลด opacity ของผู้เล่นที่ถูกเลือก
    PlayerDisplay selectedPlayerDisplay = FindPlayerDisplay(playerName);
    if (selectedPlayerDisplay != null)
    {
        selectedPlayerDisplay.SetOpacity(0.5f);
    }
}

// ฟังก์ชันหน่วงเวลา 3 วินาทีแล้วเปลี่ยนข้อความกลับเป็นปกติ
private IEnumerator ResetMessageText()
{
    yield return new WaitForSeconds(3);
    messageText.text = "สนทนากันเพื่อหาหัวหน้าภารกิจ";
}

    public void ConfirmVote()
    {
        audioManager.PlaySFX(confirmSound);
        string localPlayer = PhotonNetwork.LocalPlayer.NickName;

        if (votes.ContainsKey(localPlayer))
        {
            votes[localPlayer] = selectedPlayer;
            isVotingEnabled = false;
        }
        else
        {
            votes.Add(localPlayer, selectedPlayer);
            isVotingEnabled = false;
        }

        // นับผลโหวต
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable()
        {
            { "Votes", selectedPlayer }
        });

        Debug.Log($"{localPlayer} โหวตให้ {selectedPlayer}");

        foreach (var vote in voteCounts)
        {
            Debug.Log($"ผู้เล่น {vote.Key} มีจำนวนโหวต {vote.Value} คน");
        }

        CloseConfirmation();
    }

    // ปุ่มยกเลิกโหวตคนนี้
    public void CancelVote()
    {
        audioManager.PlaySFX(cancelSound);
        CloseConfirmation();
    }

    // ปุ่มตกลงโหวตคนนี้
    private void CloseConfirmation()
    {
        confirmationPanel.SetActive(false);

        PlayerDisplay selectedPlayerDisplay = FindPlayerDisplay(selectedPlayer);
        if (selectedPlayerDisplay != null)
        {
            selectedPlayerDisplay.SetOpacity(1f);
        }
        
        selectedPlayer = null;
    }

    // ------------------------------------------------------------------------------------------

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

        audioManager.PlaySFX(warningSound);
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
        // อัปเดตสถานะของผู้ว่าจ้าง
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "EmployerActionDone", true } });
        audioManager.PlaySFX(confirmSound);

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
        audioManager.PlaySFX(cancelSound);

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
    
    // ------------------------------------------------------------------------------------------

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

        audioManager.PlaySFX(warningSound);
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
        audioManager.PlaySFX(confirmSound);

        if (!string.IsNullOrEmpty(villainselectedPlayer))
        {
            SetVillainTarget(villainselectedPlayer);
        }

        villainConfirmationPanel.SetActive(false);
    }

    public void CancelVillainSelection()
    {
        villainConfirmationPanel.SetActive(false);
        audioManager.PlaySFX(cancelSound);

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
        PhotonNetwork.CurrentRoom.SetCustomProperties(villainAction);

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

    // ------------------------------------------------------------------------------------------

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

        Debug.Log($"[DEBUG] Employer Done: {allEmployerDone}, Villain Done: {allVillainDone}");

        if ((villainCount == 0 || allVillainDone) && (employerCount == 0 || allEmployerDone))
        {
            Debug.Log("[DEBUG] ทุกคนทำการกระทำเสร็จแล้ว กำลังเปลี่ยนฉาก...");
            StartCoroutine(ShowMissionMessageAndLoadScene());
        }
    }

    IEnumerator ShowMissionMessageAndLoadScene()
    {
        audioManager.PlaySFX(messageSound);
        messageText.text = "ได้เวลาทำภารกิจแล้ว";

        yield return new WaitForSeconds(3);
        PhotonNetwork.LoadLevel("MissionSelect");
    }
}
