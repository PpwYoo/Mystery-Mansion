using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class StartConnectPhoton : MonoBehaviourPunCallbacks
{
    public string nextSceneName = "IntroScene";
    public Text loadingText;

    public void OnClickStart()
    {
        if (loadingText != null)
        {
            loadingText.text = "กรุณารอสักครู่...";
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
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(nextSceneName);
    }
}
