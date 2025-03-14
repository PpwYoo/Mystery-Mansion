using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class JoinRoom : MonoBehaviourPunCallbacks
{
    public TMP_InputField joinRoomInputField;
    public TMP_Text status;

    public GameObject createRoomButton;
    public GameObject joinRoomButton;
    public GameObject joinConfirmButton;
    public GameObject backButton;

    [Header("BGM & SFX")]
    public AudioClip joinSceneBGM;
    public AudioClip showJoinRoomInputSound;
    public AudioClip hideJoinRoomInputSound;
    public AudioClip createRoomSound;
    public AudioClip joinRoomSound;

    private AudioManager audioManager;

    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null && joinSceneBGM != null)
        {
            audioManager.ChangeBGM(joinSceneBGM);  // เปลี่ยน BGM ตอนเข้าฉาก
        }
        
        ResetUI();
    }

    private void ResetUI()
    {
        createRoomButton.SetActive(true);
        joinRoomButton.SetActive(true);

        joinRoomInputField.gameObject.SetActive(false);
        joinConfirmButton.SetActive(false);
        backButton.SetActive(false);
    }

    public void ShowJoinRoomInput()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX(showJoinRoomInputSound);
        }

        createRoomButton.SetActive(false);
        joinRoomButton.SetActive(false);

        joinRoomInputField.gameObject.SetActive(true);
        joinConfirmButton.SetActive(true);
        backButton.SetActive(true);       
    }

    public void HideJoinRoomInput()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX(hideJoinRoomInputSound);
        }
        ResetUI();
    }

    public void CreateRoom()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX(createRoomSound);
        }
        
        string roomName = GenerateRoomName();

        // ตรวจสอบว่า NickName ถูกตั้งค่าแล้วหรือไม่
        if (!string.IsNullOrEmpty(PhotonNetwork.LocalPlayer.NickName))
        {
            PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 8 });
            PhotonNetwork.LoadLevel("GameScene");
            Debug.Log(roomName);
        }
    }

    private string GenerateRoomName()
    {
        const string chars = "0123456789";
        char[] roomName = new char[5];

        for (int i = 0; i < roomName.Length; i++)
        {
            roomName[i] = chars[Random.Range(0, chars.Length)];
        }

        return new string(roomName);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        status.text = "ไม่สามารถสร้างห้องได้: " + message;
    }

    public override void OnJoinedRoom()
    {
        if (!PhotonNetwork.CurrentRoom.IsOpen)
        {
            status.text = "ห้องนี้ถูกล็อคไปแล้ว";
            StartCoroutine(DisplayMessageForSeconds(status.text, 3f));
            PhotonNetwork.LeaveRoom();
            return;
        }
        PhotonNetwork.LoadLevel("GameScene");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        status.text = "ไม่สามารถเข้าร่วมห้อง: " + message;
        StartCoroutine(DisplayMessageForSeconds(status.text, 3f));
    }
    
    public void PlayerJoinRoom()
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX(joinRoomSound);
        }

        string roomName = joinRoomInputField.text;

        if (!string.IsNullOrEmpty(roomName) && !string.IsNullOrEmpty(PhotonNetwork.LocalPlayer.NickName))
        {
            PhotonNetwork.JoinRoom(roomName);
        }
        else
        {
            status.text = "หมายเลขห้องไม่ถูกต้อง";
            StartCoroutine(DisplayMessageForSeconds(status.text, 3f));
        }
    }

    private IEnumerator DisplayMessageForSeconds(string message, float duration)
    {
        status.text = message;
        yield return new WaitForSeconds(duration);
        status.text = "";
    }
}
