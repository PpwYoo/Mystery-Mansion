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
    public TMP_Text statusText; // ✅ เพิ่ม text สำหรับสถานะ
    public GameObject playerPrefab;
    public RectTransform[] playerPositions;
    public Button startButton;
    public Button backButton;
    private List<GameObject> currentPlayers = new List<GameObject>();

    [Header("BGM & SFX")]
    public AudioClip sceneBGM;
    public AudioClip startGameSound;

    private AudioManager audioManager;

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

        UpdateStatusText(); // ✅ เรียกใช้
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

        List<int> usedPositions = new List<int> { 0 };

        for (int i = 0; i < players.Length; i++)
        {
            int positionIndex = players[i].IsLocal ? 0 : -1;

            if (!players[i].IsLocal)
            {
                for (int j = 1; j < playerPositions.Length; j++)
                {
                    if (!usedPositions.Contains(j))
                    {
                        positionIndex = j;
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

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        SetupPlayers();
        startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        UpdateStatusText(); // ✅
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        SetupPlayers();
        startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        UpdateStatusText(); // ✅
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            roomCodeText.text = PhotonNetwork.CurrentRoom.Name;
        }
        SetupPlayers();
        startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        UpdateStatusText(); // ✅
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
