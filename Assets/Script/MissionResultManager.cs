using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MissionResultManager : MonoBehaviourPunCallbacks
{
    public static MissionResultManager Instance;

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

    public void CalculateMissionResult(string missionKey)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (missionKey != "Mission_Fingerprint" && missionKey != "Mission_SpotDifference" && missionKey != "Mission_RandomQuiz" && missionKey != "Mission_RightSigns" && missionKey != "Mission_FindTheWay") { return; }

        int totalPlayers = PhotonNetwork.PlayerList.Length;
        int successThreshold = Mathf.CeilToInt(totalPlayers / 2f) + 1;
        int successCount = CountSuccessfulPlayers(missionKey);

        bool missionPassed = successCount >= successThreshold; // คนผ่านเกินครึ่ง +1 = ภารกิจสำเร็จ
        Debug.Log($"{missionKey} result: {(missionPassed ? "Mission Passed" : "Mission Failed")}");

        ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable
        {
            { $"Result_{missionKey}", missionPassed }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
    }

    private int CountSuccessfulPlayers(string missionKey)
    {
        int successCount = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            string key = $"{missionKey}_{player.NickName}";
            if (player.CustomProperties.ContainsKey(key) && (string)player.CustomProperties[key] == "Complete")
            {
                successCount++;
            }
        }
        return successCount;
    }
}
