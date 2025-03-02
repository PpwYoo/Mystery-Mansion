using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class FindRoleSetting : MonoBehaviourPunCallbacks
{
    public static FindRoleSetting Instance;

    [Header("Find Employer Setting")]
    public GameObject villainFindEmployerPanel; public GameObject villainFindEmployerFinalPanel;

    [Header("Find Employer Final Setting")]
    public TMP_Text villainFindEmployerText; public TMP_Text villainFindEmployerFinalText;

    [Header("General find Employer")]
    public TMP_Text villainFindEmployerResult;
    private string villainSelectedEmployer;
    public bool isVillainSelectEmployerActive = false;
    public bool isVillainSelectEmployerFinalActive = false;

    [Header("General find Villain")]
    public TMP_Text findVillainResult;
    public TMP_Text allConfirmationText;
    public GameObject allConfirmationPanel;

    private string allSelectedPlayer;
    public bool isGoodSelectBadActive = false;
    private Dictionary<string, int> allVotes = new Dictionary<string, int>();
    private Dictionary<string, int> allVoteCounts = new Dictionary<string, int>();

    public float goodTimeLimit = 10f;
    private float goodTimeRemaining;
    private bool isGoodTimeActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
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

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        // ระบบหาตัวผู้ว่าจ้าง
        if (propertiesThatChanged.ContainsKey("IsVillainSelectionCorrect"))
        {
            bool isCorrect = (bool)propertiesThatChanged["IsVillainSelectionCorrect"];
            StartCoroutine(ShowResult(isCorrect));
        }
        else if (propertiesThatChanged.ContainsKey("IsVillainSelectionFinalCorrect"))
        {
            bool isCorrect = (bool)propertiesThatChanged["IsVillainSelectionFinalCorrect"];
            StartCoroutine(ShowFinalResult(isCorrect));
        }

        // ระบบหาตัวคนร้าย
        if (propertiesThatChanged.ContainsKey("FirstVillainFound"))
        {
            bool isVillainFound = (bool)propertiesThatChanged["FirstVillainFound"];
            if (isVillainFound)
            {
                Debug.Log("คนร้ายคนแรกถูกพบแล้ว!");
            }
            else
            {
                StartCoroutine(AllVoteResult()); // ถ้ายังไม่พบคนร้าย ก็ทำงานตามปกติ
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        // การเก็บค่าการโหวตหาคนร้าย
        if (changedProps.ContainsKey("allVotes"))
        {
            string votedPlayer = (string)changedProps["allVotes"];

            if (!allVoteCounts.ContainsKey(votedPlayer))
            {
                allVoteCounts[votedPlayer] = 1;
            }
            else
            {
                allVoteCounts[votedPlayer]++;
            }

            Debug.Log($"ผู้เล่น {votedPlayer} ได้รับ {allVoteCounts[votedPlayer]} โหวต");
        }
    }

    // --------------------------------------------------------------------------------------------------
    // 3 Mission Fail -> Villain find Employer

    public IEnumerator ActivateVillainHunt()
    {
        yield return new WaitForSeconds(2f);
        string[] missionFailMessages = new string[] { "ภารกิจล้มเหลวครบกำหนดแล้ว!!", "ถึงเวลาที่คนร้าย", "จะหาตัวผู้ว่าจ้าง", "คนร้ายหาตัวผู้ว่าจ้างได้เลย" };

        for (int i = 0; i < missionFailMessages.Length; i++)
        {
            GameStartII.Instance.messageText.text = missionFailMessages[i];
            yield return new WaitForSeconds(2f);
        }

        string localPlayerRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
        if (localPlayerRole == "คนร้าย")
        {
            VillainFindEmployer(villainSelectedEmployer);
        }
    }

    public void VillainFindEmployer(string playerName)
    {
        isVillainSelectEmployerActive = true;
        villainSelectedEmployer = playerName;

        if (!string.IsNullOrEmpty(playerName))
        {
            ShowFindEmployerConfirmation(playerName);
        }
    }

    public void ShowFindEmployerConfirmation(string playerName)
    {
        villainSelectedEmployer = playerName;
        villainFindEmployerText.text = "คุณมั่นใจใช่ไหม";
        villainFindEmployerPanel.SetActive(true);

        PlayerDisplay villainSelectedEmployerDisplay = FindPlayerDisplay(playerName);
        if (villainSelectedEmployerDisplay != null)
        {
            villainSelectedEmployerDisplay.SetOpacity(0.5f);
        }
    }

    public void ConfirmEmployer()
    {
        StartCoroutine(ConfirmEmployerShowResult());
    }

    private IEnumerator ConfirmEmployerShowResult()
    {
        isVillainSelectEmployerActive = false;
        villainFindEmployerPanel.SetActive(false);
        GameStartII.Instance.messageText.text = "";

        bool isCorrect = false; // เก็บผลการเลือกของคนร้าย
        PlayerDisplay villainSelectedEmployerDisplay = FindPlayerDisplay(villainSelectedEmployer);
        if (villainSelectedEmployerDisplay != null)
        {
            villainSelectedEmployerDisplay.SetOpacity(1f);
            string role = villainSelectedEmployerDisplay.playerRole;
            if (role == "ผู้ว่าจ้าง") 
            {
                isCorrect = true;
            }
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsVillainSelectionCorrect", isCorrect } });
        yield return new WaitForSeconds(2f);
        StartCoroutine(ShowResult(isCorrect));
    }

    public void CancelEmployer()
    {
        villainFindEmployerPanel.SetActive(false);

        PlayerDisplay villainSelectedEmployerDisplay = FindPlayerDisplay(villainSelectedEmployer);
        if (villainSelectedEmployerDisplay != null)
        {
            villainSelectedEmployerDisplay.SetOpacity(1f);
        }
        
        villainSelectedEmployer = null;
    }

    private IEnumerator ShowResult(bool isCorrect)
    {
        if (isCorrect)
        {
            GameStartII.Instance.messageText.text = "";
            yield return StartCoroutine(ShowEmployerResult("ถูกพบตัวแล้ว!!!"));
            PhotonNetwork.LoadLevel("Ending1");
        }
        else
        {
            GameStartII.Instance.messageText.text = "";
            yield return StartCoroutine(ShowEmployerResult("ยังไม่ถูกพบตัว!!!"));

            villainFindEmployerResult.text = "เกมยังคงดำเนินต่อไป";
            yield return new WaitForSeconds(2f);
            villainFindEmployerResult.text = "";
            yield return new WaitForSeconds(2f);
            GameStartII.Instance.StartCoroutine(GameStartII.Instance.MissionFail());
        }

        yield return null;
    }

    private IEnumerator ShowEmployerResult(string resultMessage)
    {
        string[] employerResult = new string[] { "ในตอนนี้...", "ผู้ว่าจ้าง..", resultMessage };
        for (int i = 0; i < employerResult.Length; i++)
        {
            villainFindEmployerResult.text = employerResult[i];
            yield return new WaitForSeconds(2f);
        }
    }

    // --------------------------------------------------------------------------------------------------

    public IEnumerator ActivateVillainHuntAgain()
    {
        yield return new WaitForSeconds(2f);
        string[] missionFailMessages = new string[] { "ตราบเท่าที่ภารกิจล้มเหลวมากเกินไป", "ถึงเวลาที่คนร้าย", "จะหาตัวผู้ว่าจ้าง", "คนร้ายหาตัวผู้ว่าจ้างได้เลย" };

        for (int i = 0; i < missionFailMessages.Length; i++)
        {
            GameStartII.Instance.messageText.text = missionFailMessages[i];
            yield return new WaitForSeconds(2f);
        }

        string localPlayerRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
        if (localPlayerRole == "คนร้าย")
        {
            VillainFindEmployerFinal(villainSelectedEmployer);
        }
    }

    public void VillainFindEmployerFinal(string playerName)
    {
        isVillainSelectEmployerFinalActive = true;
        villainSelectedEmployer = playerName;

        if (!string.IsNullOrEmpty(playerName))
        {
            ShowFindEmployerFinalConfirmation(playerName);
        }
    }

    public void ShowFindEmployerFinalConfirmation(string playerName)
    {
        villainSelectedEmployer = playerName;
        villainFindEmployerFinalText.text = "คุณมั่นใจใช่ไหม";
        villainFindEmployerFinalPanel.SetActive(true);

        PlayerDisplay villainSelectedEmployerDisplay = FindPlayerDisplay(playerName);
        if (villainSelectedEmployerDisplay != null)
        {
            villainSelectedEmployerDisplay.SetOpacity(0.5f);
        }
    }

    public void ConfirmFinalEmployer()
    {
        StartCoroutine(ConfirmEmployerShowResultFinal());
    }

    private IEnumerator ConfirmEmployerShowResultFinal()
    {
        isVillainSelectEmployerFinalActive = false;
        villainFindEmployerFinalPanel.SetActive(false);
        GameStartII.Instance.messageText.text = "";

        bool isCorrect = false; // เก็บผลการเลือกของคนร้าย
        PlayerDisplay villainSelectedEmployerDisplay = FindPlayerDisplay(villainSelectedEmployer);
        if (villainSelectedEmployerDisplay != null)
        {
            villainSelectedEmployerDisplay.SetOpacity(1f);
            string role = villainSelectedEmployerDisplay.playerRole;
            if (role == "ผู้ว่าจ้าง") 
            {
                isCorrect = true;
            }
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsVillainSelectionFinalCorrect", isCorrect } });
        yield return new WaitForSeconds(2f);
        StartCoroutine(ShowFinalResult(isCorrect));
    }

    public void CancelEmployerFinal()
    {
        villainFindEmployerFinalPanel.SetActive(false);

        PlayerDisplay villainSelectedEmployerDisplay = FindPlayerDisplay(villainSelectedEmployer);
        if (villainSelectedEmployerDisplay != null)
        {
            villainSelectedEmployerDisplay.SetOpacity(1f);
        }
        
        villainSelectedEmployer = null;
    }

    private IEnumerator ShowFinalResult(bool isCorrect)
    {
        if (isCorrect)
        {
            GameStartII.Instance.messageText.text = "";
            yield return StartCoroutine(ShowEmployerFinalResult("ถูกพบตัวแล้ว!!!"));
            PhotonNetwork.LoadLevel("Ending1");
        }
        else
        {
            GameStartII.Instance.messageText.text = "";
            yield return StartCoroutine(ShowEmployerFinalResult("ยังไม่ถูกพบตัว!!!"));

            villainFindEmployerResult.text = "คนร้ายแพ้แล้วล่ะ..";
            yield return new WaitForSeconds(2f);
            PhotonNetwork.LoadLevel("Ending1");
        }

        yield return null;
    }

    private IEnumerator ShowEmployerFinalResult(string resultMessage)
    {
        string[] employerResult = new string[] { "ในตอนนี้...", "ผู้ว่าจ้าง..", resultMessage };
        for (int i = 0; i < employerResult.Length; i++)
        {
            villainFindEmployerResult.text = employerResult[i];
            yield return new WaitForSeconds(2f);
        }
    }

    // --------------------------------------------------------------------------------------------------
    // 3 Mission Success -> Detective find Villain

    public IEnumerator ActivateDetectiveHunt(bool skipMessages = false)
    {
        if (allConfirmationPanel.activeSelf)
        {
            allConfirmationPanel.SetActive(false);
        }
        
        isGoodSelectBadActive = true;

        if (!skipMessages)
        {
            yield return new WaitForSeconds(2f);
            string[] missionSuccessMessages = new string[] { "ภารกิจสำเร็จครบกำหนดแล้ว!!", "ถึงเวลาที่ทุกคน", "จะโหวตหาตัวคนร้าย", "โหวตหาตัวคนร้ายได้เลย" };

            for (int i = 0; i < missionSuccessMessages.Length; i++)
            {
                GameStartII.Instance.messageText.text = missionSuccessMessages[i];
                yield return new WaitForSeconds(2f);
            }
        }

        goodTimeRemaining = goodTimeLimit;
        isGoodTimeActive = true;

        FindVillainSystem(allSelectedPlayer);
        StartCoroutine(GoodTimer());  
    }

    IEnumerator GoodTimer()
    {
        while (goodTimeRemaining > 0)
        {
            goodTimeRemaining -= Time.deltaTime;
            GameStartII.Instance.timerText.text = Utility.FormatTime(goodTimeRemaining);
            yield return null;
        }

        isGoodTimeActive = false;
        isGoodSelectBadActive = false;
        GameStartII.Instance.timerText.text = "";

        GameStartII.Instance.messageText.text = "หมดเวลาตามหาคนร้าย";
        yield return new WaitForSeconds(3f);
        GameStartII.Instance.messageText.text = "";

        StartCoroutine(AllVoteResult());
    }

    public void FindVillainSystem(string playerName)
    {
        if (!isGoodSelectBadActive || !isGoodTimeActive) return;
        allSelectedPlayer = playerName;

        if (!string.IsNullOrEmpty(playerName))
        {
            ShowAllConfirmation(playerName);
        }
    }

    public void ShowAllConfirmation(string playerName)
    {
        if (!isGoodTimeActive) return;

        allSelectedPlayer = playerName;
        allConfirmationText.text = playerName + " น่าสงสัย?";
        allConfirmationPanel.SetActive(true);

        PlayerDisplay allSelectedPlayerDisplay = FindPlayerDisplay(playerName);
        if (allSelectedPlayerDisplay != null)
        {
            allSelectedPlayerDisplay.SetOpacity(0.5f);
        }
    }

    public void ConfirmFindVillain()
    {
        if (!isGoodTimeActive || string.IsNullOrEmpty(allSelectedPlayer)) return;

        string localPlayerName = PhotonNetwork.LocalPlayer.NickName;

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable()
        {
            { "allVotes", allSelectedPlayer }
        });

        Debug.Log($"{localPlayerName} โหวตให้ {allSelectedPlayer}");

        PlayerDisplay allSelectedPlayerDisplay = FindPlayerDisplay(allSelectedPlayer);
        if (allSelectedPlayerDisplay != null)
        {
            allSelectedPlayerDisplay.SetOpacity(1f);
        }

        allConfirmationPanel.SetActive(false);
        isGoodSelectBadActive = false;
    }

    public void CancelFindVillain()
    {
        if (!isGoodTimeActive || string.IsNullOrEmpty(allSelectedPlayer)) return;
        allConfirmationPanel.SetActive(false);

        PlayerDisplay allSelectedPlayerDisplay = FindPlayerDisplay(allSelectedPlayer);
        if (allSelectedPlayerDisplay != null)
        {
            allSelectedPlayerDisplay.SetOpacity(1f);
        }
        
        allSelectedPlayer = null;   
    }

    public IEnumerator AllVoteResult()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("FirstVillainFound", out object value) && (bool)value)
        {
            Debug.Log("ดำเนินการหาคนร้ายคนสุดท้าย");
            StartCoroutine(EndGameProcess());
            yield break;
        }

        int currentRound = 0;
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("CurrentRound"))
        {
            currentRound = (int)PhotonNetwork.CurrentRoom.CustomProperties["CurrentRound"];
        }
        Debug.Log("รอบปัจจุบันที่ดึงมา: " + currentRound);

        // ถ้าไม่มีการโหวตใดๆ
        if (allVoteCounts.Count == 0)
        {
            findVillainResult.text = "ไม่มีใครถูกโหวต!";
            yield return new WaitForSeconds(2f);
            findVillainResult.text = "";

            GameStartII.Instance.StartDiscuss();
            yield break;
        }

        // หาผู้เล่นที่ได้รับโหวตมากที่สุด
        var mostVotedPlayers = allVoteCounts.OrderByDescending(kvp => kvp.Value).ToList();
        int highestVotes = mostVotedPlayers.First().Value;

        // ตรวจสอบว่ามีผู้เล่นหลายคนที่ได้รับคะแนนโหวตเท่ากันหรือไม่
        var tiedPlayers = mostVotedPlayers.Where(kvp => kvp.Value == highestVotes).Select(kvp => kvp.Key).ToList();

        if (tiedPlayers.Count > 1)
        {
            findVillainResult.text = "ผู้เล่นมีคะแนนโหวต\nเท่ากัน";
            yield return new WaitForSeconds(2f);
            findVillainResult.text = "ทำให้ไม่มีผู้เล่น\nถูกตรวจสอบ!";
            yield return new WaitForSeconds(2f);
            findVillainResult.text = "";

            GameStartII.Instance.StartDiscuss(); 
            yield break;
        }

        string mostVotedPlayer = mostVotedPlayers.First().Key;
        PlayerDisplay allSelectedPlayerDisplay = FindPlayerDisplay(mostVotedPlayer);

        if (allSelectedPlayerDisplay != null)
        {
            string role = allSelectedPlayerDisplay.playerRole;
            yield return new WaitForSeconds(2f);

            if (role == "นักสืบ" || role == "ผู้ว่าจ้าง")
            {
                yield return new WaitForSeconds(2f);
                string[] villainResult = currentRound == 5 
                    ? new string[] { $"ผู้เล่น {mostVotedPlayer}", "...", "ไม่ใช่คนร้าย!!", "คนร้ายยังคงเหลืออยู่..", "คุณไม่สามารถ\nหาคนร้ายได้เลย" }
                    : new string[] { $"ผู้เล่น {mostVotedPlayer}", "...", "ไม่ใช่คนร้าย!!", "เกมยังคงดำเนินต่อไป", "" };

                foreach (string message in villainResult)
                {
                    findVillainResult.text = message;
                    yield return new WaitForSeconds(2f);
                }

                if (currentRound == 5)
                {
                    PhotonNetwork.LoadLevel("Ending2");
                }
                else
                {
                    GameStartII.Instance.StartDiscuss();
                }

                yield break;
            }
            else if (role == "คนร้าย")
            {
                yield return new WaitForSeconds(2f);
                string[] villainResult = new string[] { $"ผู้เล่น {mostVotedPlayer}", "...", "คือคนร้าย!!", "คุณพบตัวคนร้ายแล้ว!!", "" };

                foreach (string message in villainResult)
                {
                    findVillainResult.text = message;
                    yield return new WaitForSeconds(2f);
                }

                ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable();
                roomProperties["FirstVillainFound"] = true;
                PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

                if (currentRound == 5)
                {
                    yield return new WaitForSeconds(2f);
                    findVillainResult.text = "คุณมีโอกาส\nโหวตอีกครั้ง!";
                    yield return new WaitForSeconds(2f);

                    StartCoroutine(ActivateDetectiveHunt(true));
                    yield break;
                }

                GameStartII.Instance.StartDiscuss();
                yield break;
            }
        }
        else
        {
            Debug.LogError($"ไม่พบข้อมูลของ {mostVotedPlayer}");
        }
    }

    public IEnumerator EndGameProcess()
    {
        int currentRound = 0;
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("CurrentRound"))
        {
            currentRound = (int)PhotonNetwork.CurrentRoom.CustomProperties["CurrentRound"];
        }
        Debug.Log("รอบปัจจุบันที่ดึงมา: " + currentRound);

        if (currentRound == 5)
        {
            Debug.Log("จุดตัดสินฉากจบ");
            yield return StartCoroutine(ProcessVotingResults(true));
            yield break;
        }

        yield return StartCoroutine(ProcessVotingResults(false));
    }

    public IEnumerator ProcessVotingResults(bool isFinalRound)
    {
        if (allVoteCounts.Count == 0)
        {
            findVillainResult.text = "ไม่มีใครถูกโหวต!";
            yield return new WaitForSeconds(2f);
            findVillainResult.text = isFinalRound ? "คนร้ายยังคงเหลืออยู่.." : "";
            yield return new WaitForSeconds(2f);

            if (isFinalRound)
            {
                PhotonNetwork.LoadLevel("Ending2");
            }
            else
            {
                GameStartII.Instance.StartDiscuss();
            }
            yield break;
        }

        // หาผู้เล่นที่ได้รับโหวตมากที่สุด
        var mostVotedPlayers = allVoteCounts.OrderByDescending(kvp => kvp.Value).ToList();
        int highestVotes = mostVotedPlayers.First().Value;

        // ตรวจสอบว่ามีผู้เล่นที่ได้คะแนนโหวตเท่ากันหรือไม่
        var tiedPlayers = mostVotedPlayers.Where(kvp => kvp.Value == highestVotes).Select(kvp => kvp.Key).ToList();

        if (tiedPlayers.Count > 1)
        {
            findVillainResult.text = "ผู้เล่นมีคะแนนโหวต\nเท่ากัน";
            yield return new WaitForSeconds(2f);
            findVillainResult.text = "ทำให้ไม่มีผู้เล่น\nถูกตรวจสอบ!";
            yield return new WaitForSeconds(2f);
            findVillainResult.text = isFinalRound ? "คนร้ายยังคงเหลืออยู่.." : "";
            yield return new WaitForSeconds(2f);

            if (isFinalRound)
            {
                PhotonNetwork.LoadLevel("Ending2");
            }
            else
            {
                GameStartII.Instance.StartDiscuss();
            }
            yield break;
        }

        string mostVotedPlayer = mostVotedPlayers.First().Key;
        PlayerDisplay allSelectedPlayerDisplay = FindPlayerDisplay(mostVotedPlayer);

        if (allSelectedPlayerDisplay != null)
        {
            string role = allSelectedPlayerDisplay.playerRole;
            yield return new WaitForSeconds(2f);

            if (role == "นักสืบ" || role == "ผู้ว่าจ้าง")
            {
                string[] villainResult = isFinalRound 
                    ? new string[] { $"ผู้เล่น {mostVotedPlayer}", "...", "ไม่ใช่คนร้าย!!", "คนร้ายยังคงเหลืออยู่.." }
                    : new string[] { $"ผู้เล่น {mostVotedPlayer}", "...", "ไม่ใช่คนร้าย!!", "เกมยังคงดำเนินต่อไป", "" };

                foreach (string message in villainResult)
                {
                    findVillainResult.text = message;
                    yield return new WaitForSeconds(2f);
                }

                if (isFinalRound)
                {
                    PhotonNetwork.LoadLevel("Ending2");
                }
                else
                {
                    GameStartII.Instance.StartDiscuss();
                }

                yield break;
            }
            else if (role == "คนร้าย")
            {
                yield return new WaitForSeconds(2f);
                string[] villainResult = new string[] { $"ผู้เล่น {mostVotedPlayer}", "...", "คือคนร้าย!!", "คุณพบตัวคนร้าย\nครบแล้ว!!" };

                foreach (string message in villainResult)
                {
                    findVillainResult.text = message;
                    yield return new WaitForSeconds(2f);
                }

                PhotonNetwork.LoadLevel(isFinalRound ? "Ending1" : "Ending2");
                yield break;
            }
        }
        else
        {
            Debug.LogError($"ไม่พบข้อมูลของ {mostVotedPlayer}");
        }
    }

    // --------------------------------------------------------------------------------------------------

    // เปลี่ยน Sprite ของผู้เล่นที่ถูกเลือกไปแล้ว
    private void ChangePlayerDisplay(PlayerDisplay playerDisplay)
    {
        Sprite newSprite = Resources.Load<Sprite>("KilledPlayer");
         
        if (newSprite != null)
        {
            if (playerDisplay.playerDisplay != null)
            {
                playerDisplay.playerDisplay.sprite = newSprite;
            }
            else
            {
                Debug.LogError("ไม่พบ Image ใน PlayerDisplay");
            }
        }
        else
        {
            Debug.LogError("ไม่สามารถโหลด Sprite ได้จาก Resources");
        }
    }

    // ปิดการเลือกผู้เล่นที่ถูกเลือกไปแล้ว
    private void DisablePlayerSelection(string playerName)
    {
        PlayerDisplay targetDisplay = FindPlayerDisplay(playerName);
    
        if (targetDisplay != null)
        {
            targetDisplay.GetComponent<Button>().interactable = false;
        }
    }

}
