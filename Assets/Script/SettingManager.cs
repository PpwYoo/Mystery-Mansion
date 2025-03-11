using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    public GameObject instructionPanel;
    public ScrollRect instructionScrollRect;
    public GameObject rulePanel;
    public GameObject rolePanel;

    private GameObject currentOpenPanel;

    [Header("SFX Sounds")]
    public AudioClip showPanelSound;
    public AudioClip closeButtonSound;

    private AudioManager audioManager;

    public void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
    }

    public void ShowInstructionPanel()
    {
        audioManager.PlaySFX(showPanelSound);

        CloseCurrentPanel();
        instructionPanel.SetActive(true);
        ResetScrollPosition();
        currentOpenPanel = instructionPanel;
    }

    public void ShowRulePanel()
    {
        audioManager.PlaySFX(showPanelSound);

        CloseCurrentPanel();
        rulePanel.SetActive(true);
        currentOpenPanel = rulePanel;
    }

    public void ShowRolePanel()
    {
        audioManager.PlaySFX(showPanelSound);

        CloseCurrentPanel();
        rolePanel.SetActive(true);
        currentOpenPanel = rolePanel;
    }

    public void CloseCurrentPanel()
    {
        if (currentOpenPanel != null)
        {
            audioManager.PlaySFX(closeButtonSound);

            currentOpenPanel.SetActive(false);
            currentOpenPanel = null;
        }
    }

    private void ResetScrollPosition()
    {
        if (instructionScrollRect != null)
        {
            instructionScrollRect.verticalNormalizedPosition = 1f;
        }
    }
}
