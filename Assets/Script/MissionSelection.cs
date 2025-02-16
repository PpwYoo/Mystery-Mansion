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

    void Start()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Leader"))
        {
            string leader = (string)PhotonNetwork.CurrentRoom.CustomProperties["Leader"];
            Debug.Log($"หัวหน้าภารกิจคือ: {leader}");

            // ทุกคนสามารถเลือกภารกิจเพื่อดูรายละเอียดได้
            foreach (Button button in missionButtons)
            {
                button.interactable = true; 
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
    }

    public void CloseMissionPanel(int missionIndex)
    {
        missionPanels[missionIndex].SetActive(false);
    }

    // เริ่มภารกิจ
    public void StartMission(int sceneIndex)
    {
        if (PhotonNetwork.LocalPlayer.NickName == (string)PhotonNetwork.CurrentRoom.CustomProperties["Leader"])
        {
            this.sceneIndex = sceneIndex;

            ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable()
            {
                { "SelectedMission", sceneIndex }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }
        else
        {
            Debug.Log("เฉพาะหัวหน้าภารกิจที่สามารถเริ่มภารกิจได้");
        }
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

