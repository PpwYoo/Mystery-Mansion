using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    public GameObject instructionPanel;
    public ScrollRect instructionScrollRect;
    public GameObject rulePanel;
    public GameObject rolePanel;

    private GameObject currentOpenPanel;

    public void ShowInstructionPanel()
    {
        CloseCurrentPanel();
        instructionPanel.SetActive(true);
        ResetScrollPosition();
        currentOpenPanel = instructionPanel;
    }

    public void ShowRulePanel()
    {
        CloseCurrentPanel();
        rulePanel.SetActive(true);
        currentOpenPanel = rulePanel;
    }

    public void ShowRolePanel()
    {
        CloseCurrentPanel();
        rolePanel.SetActive(true);
        currentOpenPanel = rolePanel;
    }

    public void CloseCurrentPanel()
    {
        if (currentOpenPanel != null)
        {
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
