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
    public RectTransform[] playerPositions;
    private List<GameObject> currentPlayers = new List<GameObject>();

    [Header("Mission Setting")]
    public Image[] missionResultImages;
    public Sprite successSprite;
    public Sprite failSprite;
    private int missionIndex = 0;

    void Start()
    {
        SetupPlayers();
        
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("LastMission", out object lastMissionKey))
        {
            Debug.Log($"Last Mission: {lastMissionKey}");
            ShowMissionResult((string)lastMissionKey);
        }
        else
        {
            messageText.text = "ยังไม่มีผลภารกิจ";
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

    void ShowMissionResult(string missionKey)
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue($"Result_{missionKey}", out object result))
        {
            bool isMissionSuccess = (bool)result;
            messageText.text = isMissionSuccess ? "ภารกิจสำเร็จ" : "ภารกิจล้มเหลว";
            Debug.Log($"{missionKey} result: {(isMissionSuccess ? "Success" : "Fail")}");

            if (missionIndex < missionResultImages.Length)
            {
                missionResultImages[missionIndex].sprite = isMissionSuccess ? successSprite : failSprite;
                missionIndex++;
            }
        }
        else
        {
            messageText.text = "ยังไม่มีผลภารกิจ";
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("Result_Mission_SpotDifference"))
        {
            ShowMissionResult("Mission_SpotDifference");
        }

        if (propertiesThatChanged.ContainsKey("Result_Mission_FindTheWay"))
        {
            ShowMissionResult("Mission_FindTheWay");
        }
    }
}
