using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class EndingScene : MonoBehaviourPunCallbacks
{
    public AudioClip sceneBGM;
    public AudioClip backHomeSound;

    private AudioManager audioManager;

    void Start()
    {   
        audioManager = FindObjectOfType<AudioManager>();

        if (AudioManager.instance != null)
        {
            AudioManager.instance.ChangeBGM(sceneBGM);
        }
    }

    // Back Home
    public void BackHome()
    {
        audioManager.PlaySFX(backHomeSound);
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Invoke("LoadStartScene", 2f);
    }

    void LoadStartScene()
    {
        PhotonNetwork.LoadLevel("StartScene");
    }
}
