using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;

public class StartConnectPhoton : MonoBehaviourPunCallbacks
{
    public string nextSceneName = "IntroScene";
    public TMP_Text loadingText;

    public void OnClickStart()
    {
        if (loadingText != null)
        {
            loadingText.text = "LOADING...";
        }
        ConnectToPhoton();
    }

    void ConnectToPhoton()
    {
        PhotonNetwork.ConnectUsingSettings(); // Start connecting
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        StartCoroutine(LoadNextSceneWithDelay());
    }

    private IEnumerator LoadNextSceneWithDelay()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(nextSceneName);
    }
}
