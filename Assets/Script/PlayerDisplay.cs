using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerDisplay : MonoBehaviour
{
    public TMP_Text playerNameText;
    public TMP_Text playerRoleText;
    public Image playerDisplay;
    public Image leaderIcon;
    public Button playerButton;

    public string playerName;
    public string playerRole;

    private GameStart voteManager;
    private GameStart employerManager;
    private GameStart villainManager;

    void Start()
    {
        
        if (leaderIcon != null)
        {
            leaderIcon.gameObject.SetActive(false);
        }

        voteManager = FindObjectOfType<GameStart>();

        if (playerButton != null)
        {
            playerButton.onClick.AddListener(OnPlayerSelected);
        }

        employerManager = FindObjectOfType<GameStart>();
        villainManager = FindObjectOfType<GameStart>();
    }


    public void SetPlayerInfo(string playerName)
    {
        this.playerName = playerName; 
        playerNameText.text = playerName;
    }

    public void SetPlayerRole(string playerRole)
    {
        this.playerRole = playerRole;

        if (PhotonNetwork.LocalPlayer.NickName == this.playerName)
        {
            playerRoleText.text = playerRole;
        }
        else
        {
            playerRoleText.text = " ";
        }
    }

    public void ShowLeaderIcon(bool isLeader)
    {
        if (leaderIcon != null)
        {
            leaderIcon.gameObject.SetActive(isLeader);
        }
    }

    private void OnPlayerSelected()
    {
        if (voteManager != null && !string.IsNullOrEmpty(playerName))
        {
            if (!voteManager.isEmployerSelectionActive && !voteManager.isVillainSelectionActive)
            {
                voteManager.OnCaptainSelected(playerName);
            }
        }

        if (employerManager != null && !string.IsNullOrEmpty(playerName))
        {
            if (voteManager.isEmployerSelectionActive)
            {
                employerManager.PerformActionForEmployer(playerName);
            }
        }

        if (villainManager != null && !string.IsNullOrEmpty(playerName))
        {
            if (voteManager.isVillainSelectionActive)
            {
                villainManager.PerformActionForVillain(playerName);
            }
        }
    }

    public void SetOpacity(float alpha)
    {
        if (playerDisplay != null)
        {
            Color color = playerDisplay.color;
            color.a = alpha;
            playerDisplay.color = color;
        }

        if (playerButton != null)
        {
            playerButton.interactable = true;
        }
    }
}
