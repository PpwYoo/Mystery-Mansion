using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameScene : MonoBehaviourPunCallbacks
{
    public TMP_Text roomCodeText;
    public TMP_Text statusText;
    public GameObject playerPrefab;
    public RectTransform[] playerPositions;
    public Button startButton;
    public Button backButton;
    public TMP_Text playerCountText;
    private List<GameObject> currentPlayers = new List<GameObject>();

    [Header("BGM & SFX")]
    public AudioClip sceneBGM;
    public AudioClip startGameSound;

    private AudioManager audioManager;
    private Coroutine updatePlayersCoroutine;

    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();

        if (AudioManager.instance != null)
{
    AudioManager.instance.ChangeBGM(sceneBGM);
}

        if (PhotonNetwork.CurrentRoom != null)
        {
            roomCodeText.text = PhotonNetwork.CurrentRoom.Name;
        }

        SetupPlayers();
        PhotonNetwork.AutomaticallySyncScene = true;

        startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        startButton.onClick.AddListener(StartGame);
        backButton.onClick.AddListener(LeaveRoom);

        UpdateStatusText();
        UpdatePlayerCountText();
    }

    void UpdateStatusText()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            statusText.text = "ถ้าพร้อมแล้วกดเริ่มเกมได้เลย";
        }
        else
        {
            statusText.text = "กรุณารอสักครู่...";
        }
    }

    void UpdatePlayerCountText()
{
    if (PhotonNetwork.CurrentRoom != null)
    {
        int currentPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        playerCountText.text = $"ผู้เล่น {currentPlayers}/{maxPlayers} คน";
    }
}

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("JoinGameScene");
    }

    void SetupPlayers()
    {
        ClearExistingPlayers();

        Player[] players = PhotonNetwork.PlayerList;
        HashSet<string> playerNicknames = new HashSet<string>();
        List<int> usedPositions = new List<int>();

        for (int i = 0; i < players.Length; i++)
        {
            if (playerNicknames.Contains(players[i].NickName)) continue;
            playerNicknames.Add(players[i].NickName);

            int positionIndex = -1;

            if (players[i].IsLocal)
            {
                positionIndex = 0;
                usedPositions.Add(0);
            }
            else
            {
                for (int j = 1; j < playerPositions.Length; j++)
                {
                    if (!usedPositions.Contains(j))
                    {
                        positionIndex = j;
                        usedPositions.Add(j);
                        break;
                    }
                }
            }

            if (positionIndex != -1)
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

    private void SafeUpdatePlayers()
    {
        if (updatePlayersCoroutine != null)
        {
            StopCoroutine(updatePlayersCoroutine);
        }
        updatePlayersCoroutine = StartCoroutine(DelayedSetupPlayers());
    }

    private IEnumerator DelayedSetupPlayers()
    {
        yield return new WaitForSeconds(0.1f); // รอให้ Photon sync เสร็จ
        SetupPlayers();
        startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        UpdateStatusText();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        SafeUpdatePlayers();
        UpdatePlayerCountText();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        SafeUpdatePlayers();
        UpdatePlayerCountText();
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            roomCodeText.text = PhotonNetwork.CurrentRoom.Name;
        }
        SafeUpdatePlayers();
        UpdatePlayerCountText();
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            audioManager.PlaySFX(startGameSound);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel("GameStart");
        }
    }
}
