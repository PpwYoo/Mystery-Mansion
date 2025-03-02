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

    private GameStart gameStartManager;
    private GameStartII gameStartIIManager;
    private FindRoleSetting findRoleSetting;

    void Start()
    {
        if (leaderIcon != null)
        {
            leaderIcon.gameObject.SetActive(false);
        }

        gameStartManager = FindObjectOfType<GameStart>();
        gameStartIIManager = FindObjectOfType<GameStartII>();
        findRoleSetting = FindObjectOfType<FindRoleSetting>();

        if (playerButton != null)
        {
            playerButton.onClick.AddListener(OnPlayerSelected);
        }
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
        if (!string.IsNullOrEmpty(playerName))
        {
            if (gameStartManager != null)
            {
                if (!gameStartManager.isEmployerSelectionActive && !gameStartManager.isVillainSelectionActive)
                {
                    gameStartManager.OnCaptainSelected(playerName);
                }
                if (gameStartManager.isEmployerSelectionActive)
                {
                    gameStartManager.PerformActionForEmployer(playerName);
                }
                if (gameStartManager.isVillainSelectionActive)
                {
                    gameStartManager.PerformActionForVillain(playerName);
                }
            }
            else if (gameStartIIManager != null)
            {
                if (gameStartIIManager.isEmployerSelectionActive)
                {
                    gameStartIIManager.PerformActionForEmployer(playerName);
                }
                if (gameStartIIManager.isVillainSelectionActive)
                {
                    gameStartIIManager.PerformActionForVillain(playerName);
                }
                if (gameStartIIManager.issusSelectionActive)
                {
                    gameStartIIManager.MissionFailSystem(playerName);
                }
                if (findRoleSetting.isVillainSelectEmployerActive)
                {
                    findRoleSetting.VillainFindEmployer(playerName);
                }
                if (findRoleSetting.isVillainSelectEmployerFinalActive)
                {
                    findRoleSetting.VillainFindEmployerFinal(playerName);
                }
                if (findRoleSetting.isGoodSelectBadActive)
                {
                    findRoleSetting.FindVillainSystem(playerName);
                }
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
