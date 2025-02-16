using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpotsHuntSetting : MonoBehaviourPunCallbacks
{
    public static SpotsHuntSetting Instance;

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

        int totalPlayers = PhotonNetwork.PlayerList.Length;
        int successThreshold = Mathf.CeilToInt(totalPlayers / 2f);
        int successCount = CountSuccessfulPlayers(missionKey);

        bool missionPassed = successCount >= successThreshold;

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
