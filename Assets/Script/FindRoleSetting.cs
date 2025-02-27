using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

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

    public void ActivateDetectiveHunt()
    {
        Debug.Log("[MissionFailureManager] 3 Missions Succeeded! Detectives can now hunt the Villains!");
    }
}
