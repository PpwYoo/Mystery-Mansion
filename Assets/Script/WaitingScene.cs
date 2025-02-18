using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class WaitingScene : MonoBehaviour
{
    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props["CurrentScene"] = "WaitingScene";
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

            StartCoroutine(CheckAllPlayersReady());
        }
        else
        {
            Debug.Log("ยังไม่ได้เชื่อมต่อกับห้อง Waiting Room");
        }
    }

    IEnumerator CheckAllPlayersReady()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);

            int waitingPlayers = 0;
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (player.CustomProperties.ContainsKey("CurrentScene") && (string)player.CustomProperties["CurrentScene"] == "WaitingScene")
                {
                    waitingPlayers++;
                }
            }

            if (waitingPlayers == PhotonNetwork.PlayerList.Length)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("CurrentMission", out object missionKey))
                    {
                        Debug.Log($"Current Mission: {missionKey}");
                        string missionName = missionKey.ToString();
                        MissionResultManager.Instance.CalculateMissionResult(missionName);

                        ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable
                        {
                            { "LastMission", missionName }
                        };
                        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
                    }
                }

                Invoke("ChangeToNextScene", 2f);
                yield break;
            }
        }
    }

    void ChangeToNextScene()
    {
        PhotonNetwork.LoadLevel("GameStartII");
    }
}
