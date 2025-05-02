using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatCanvasController : MonoBehaviour
{
    public Button toggleChatButton;
    public Button sendButton;
    public TMP_InputField inputField;

    void Start()
    {
        if (toggleChatButton != null)
            toggleChatButton.onClick.AddListener(() => ChatManager.Instance.ToggleChatPanel());

        if (sendButton != null)
            sendButton.onClick.AddListener(() => ChatManager.Instance.OnSendButton());

        if (inputField != null)
            inputField.onEndEdit.AddListener(OnInputEndEdit);
    }

    void OnInputEndEdit(string value)
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ChatManager.Instance.OnSendButton();
        }
    }
}
