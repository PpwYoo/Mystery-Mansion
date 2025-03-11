using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class MissionSelection : MonoBehaviourPunCallbacks
{
    public Button[] missionButtons;
    public Button[] startMissionButtons;
    public GameObject[] missionPanels;
    private int sceneIndex;

    [Header("SFX")]
    public AudioClip openMissionSound;
    public AudioClip closeMissionSound;
    public AudioClip startMissionSound;

    private AudioManager audioManager;

    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Leader"))
        {
            string leader = (string)PhotonNetwork.CurrentRoom.CustomProperties["Leader"];

            // ทุกคนสามารถเลือกภารกิจเพื่อดูรายละเอียดได้
            foreach (Button button in missionButtons)
            {
                button.interactable = true; 
            }

            // ปิดปุ่มภารกิจที่เคยเล่นไปแล้ว
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("PlayedMissions"))
            {
                int[] playedMissions = (int[])PhotonNetwork.CurrentRoom.CustomProperties["PlayedMissions"];
                foreach (int index in playedMissions)
                {
                    missionButtons[index].interactable = false;
                }
            }

            // เฉพาะหัวหน้าภารกิจที่สามารถเลือกภารกิจได้
            if (PhotonNetwork.LocalPlayer.NickName == leader)
            {
                foreach (Button startButton in startMissionButtons)
                {
                    startButton.interactable = true;
                }
            }
            else
            {
                foreach (Button startButton in startMissionButtons)
                {
                    startButton.interactable = false;
                }
            }
        }
        else
        {
            Debug.Log("ไม่มีหัวหน้าภารกิจในห้องนี้");
        }
    }

    // เลือกภารกิจ
    public void ShowMissionDetails(int missionIndex)
    {
        foreach (GameObject panel in missionPanels)
        {
            panel.SetActive(false);
        }
        missionPanels[missionIndex].SetActive(true);
        audioManager.PlaySFX(openMissionSound);
    }

    public void CloseMissionPanel(int missionIndex)
    {
        missionPanels[missionIndex].SetActive(false);
        audioManager.PlaySFX(closeMissionSound);
    }

    // เริ่มภารกิจ
    public void StartMission(int sceneIndex)
    {
        if (PhotonNetwork.LocalPlayer.NickName == (string)PhotonNetwork.CurrentRoom.CustomProperties["Leader"])
        {
            this.sceneIndex = sceneIndex;

            // ดึงภารกิจที่เล่นไปแล้ว (ถ้ามี)
            int[] playedMissions = new int[0];
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("PlayedMissions"))
            {
                playedMissions = (int[])PhotonNetwork.CurrentRoom.CustomProperties["PlayedMissions"];
            }

            List<int> playedMissionsList = new List<int>(playedMissions);

            // เพิ่มภารกิจใหม่เข้าไปในลิสต์
            if (!playedMissionsList.Contains(sceneIndex))
            {
                playedMissionsList.Add(sceneIndex);
            }  

            ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable()
            {
                { "SelectedMission", sceneIndex },{ "PlayedMissions", playedMissionsList.ToArray() }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }
        else
        {
            Debug.Log("เฉพาะหัวหน้าภารกิจที่สามารถเริ่มภารกิจได้");
        }

        audioManager.PlaySFX(startMissionSound);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);

        if (propertiesThatChanged.ContainsKey("SelectedMission"))
        {
            sceneIndex = (int)propertiesThatChanged["SelectedMission"];
            Invoke("LoadMissionScene", 2f);
        }
    }

    private void LoadMissionScene()
    {
        string sceneName = GetSceneName(sceneIndex);
        PhotonNetwork.LoadLevel(sceneName);
    }

    // Mission Scene Name
    private string GetSceneName(int index)
    {
        switch (index)
        {
            case 0: return "Fingerprint";
            case 1: return "SpotsHunt";
            case 2: return "RandomQuiz";
            case 3: return "RightSigns";
            case 4: return "FindtheWay";
            default: return "MissionSelect";
        }
    }
}

