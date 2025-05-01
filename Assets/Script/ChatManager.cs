using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class ChatManager : MonoBehaviourPunCallbacks
{
    public static ChatManager Instance;

    [Header("Chat UI")]
    public GameObject chatPanel;
    public TMP_InputField inputField;
    public Transform messageContent;
    public GameObject messagePrefab;
    public ScrollRect scrollRect;
    public GameObject chatNotificationIcon;
    public TMP_Text notificationCountText;
    public GameObject noMessageText;

    [Header("Clear Chat Confirmation")]
    public GameObject clearChatDialog;

    private Queue<GameObject> messageQueue = new Queue<GameObject>();
    private int maxMessages = 100;
    private bool isChatOpen = false;
    private int unreadCount = 0;

    // เพิ่มส่วนที่ใช้เก็บข้อมูลชื่อผู้เล่นและสี
    private Dictionary<string, string> userColorMap = new Dictionary<string, string>();
    private bool hasStartedChatting = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        chatPanel.SetActive(false);
        chatNotificationIcon.SetActive(false);
        noMessageText.SetActive(true); // ข้อความ "ยังไม่มีการเริ่มบทสนทนา"
        UpdateNotificationText();
    }

    void Update()
    {
        if (isChatOpen && inputField.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            OnSendButton();
        }

        // เพิ่มการตรวจสอบเมื่อแป้นพิมพ์ปรากฏขึ้น
        if (TouchScreenKeyboard.visible)
        {
            // ปรับตำแหน่งของ ScrollRect ให้อยู่ในตำแหน่งที่ไม่ถูกบัง
            scrollRect.verticalNormalizedPosition = 0f; // เลื่อนข้อความขึ้น
        }
    }

    public void OnSendButton()
    {
        string rawText = inputField.text;

        if (!string.IsNullOrWhiteSpace(rawText) && CountThaiBaseCharacters(rawText) > 0)
        {
            if (HasExcessiveDuplicateCharacters(rawText, 10))
            {
                Debug.Log("ข้อความมีตัวอักษรหรือสระซ้ำเกิน 10 ตัว");
                return;
            }

            photonView.RPC("ReceiveMessage", RpcTarget.All, PhotonNetwork.NickName, rawText);
            inputField.text = "";
            inputField.ActivateInputField();

            // ลบข้อความที่บอกว่าใครล้างแชท
            if (messageQueue.Count > 0)
            {
                GameObject firstMessage = messageQueue.Peek();
                if (firstMessage != null && firstMessage.GetComponent<TMP_Text>().text.Contains("ได้ล้างข้อความแชท"))
                {
                    Destroy(firstMessage); // ลบข้อความที่บอกว่าใครล้างแชท
                    messageQueue.Dequeue();
                    UpdateNoMessageTextVisibility();
                }
            }
        }
    }

    [PunRPC]
    void ReceiveMessage(string senderName, string messageText)
    {
        // เช็คว่าเป็นครั้งแรกของผู้เล่นนี้หรือไม่ ถ้าใช่ให้สุ่มสี
        if (!userColorMap.ContainsKey(senderName))
        {
            userColorMap[senderName] = GetRandomColorHex();
        }

        string colorHex = userColorMap[senderName];
        string coloredName = $"<b><color={colorHex}>{senderName}</color></b>";

        string finalMessage = $"{coloredName}: {messageText}";

        GameObject msgObj = Instantiate(messagePrefab, messageContent);
        msgObj.GetComponent<TMP_Text>().text = finalMessage;

        messageQueue.Enqueue(msgObj);
        if (messageQueue.Count > maxMessages)
        {
            Destroy(messageQueue.Dequeue());
        }

        UpdateNoMessageTextVisibility();
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f; // เลื่อนข้อความไปที่ด้านล่าง

        if (!isChatOpen)
        {
            unreadCount++;
            chatNotificationIcon.SetActive(true);
            UpdateNotificationText();
        }

        // เมื่อเริ่มการพิมพ์ข้อความครั้งแรก ให้เปลี่ยนสถานะ
        hasStartedChatting = true;
        noMessageText.SetActive(false); // ซ่อนข้อความ "ยังไม่มีการเริ่มบทสนทนา"
    }

    public void ToggleChatPanel()
    {
        isChatOpen = !isChatOpen;
        chatPanel.SetActive(isChatOpen);

        if (isChatOpen)
        {
            chatNotificationIcon.SetActive(false);
            unreadCount = 0;
            UpdateNotificationText();
            inputField.ActivateInputField();
            UpdateNoMessageTextVisibility();
        }
    }

    public bool isChatPanelOpen()
    {
        return chatPanel.activeSelf;
    }

    private int CountThaiBaseCharacters(string input)
    {
        HashSet<char> ignoreChars = new HashSet<char>
        {
            '\u0E31', '\u0E34', '\u0E35', '\u0E36', '\u0E37', '\u0E38', '\u0E39',
            '\u0E47', '\u0E48', '\u0E49', '\u0E4A', '\u0E4B', '\u0E4C', '\u0E4D',
            '\u0E4E', '\u0E3A', '\u200B'
        };

        int count = 0;
        foreach (char c in input)
        {
            if (!ignoreChars.Contains(c) && !char.IsWhiteSpace(c))
            {
                count++;
            }
        }
        return count;
    }

    private bool HasExcessiveDuplicateCharacters(string input, int maxDuplicates)
    {
        HashSet<char> ignoreChars = new HashSet<char>
        {
            '\u0E31', '\u0E34', '\u0E35', '\u0E36', '\u0E37', '\u0E38', '\u0E39',
            '\u0E47', '\u0E48', '\u0E49', '\u0E4A', '\u0E4B', '\u0E4C', '\u0E4D',
            '\u0E4E', '\u0E3A', '\u200B'
        };

        Dictionary<char, int> characterCount = new Dictionary<char, int>();

        foreach (char c in input)
        {
            if (char.IsWhiteSpace(c) || ignoreChars.Contains(c)) continue;

            if (characterCount.ContainsKey(c))
            {
                characterCount[c]++;
                if (characterCount[c] > maxDuplicates)
                {
                    return true;
                }
            }
            else
            {
                characterCount[c] = 1;
            }
        }

        return false;
    }

    private void UpdateNoMessageTextVisibility()
    {
        // หากมีข้อความในแชทจะไม่แสดงข้อความ "ยังไม่มีการเริ่มบทสนทนา"
        noMessageText.SetActive(messageQueue.Count == 0 && !hasStartedChatting);
    }

    private void UpdateNotificationText()
    {
        if (notificationCountText != null)
        {
            notificationCountText.text = unreadCount > 0 ? unreadCount.ToString() : "";
        }
    }

    // ========== Clear Chat Feature ==========

    public void ShowClearChatConfirmation()
    {
        clearChatDialog.SetActive(true);
    }

    public void CancelClearChat()
    {
        clearChatDialog.SetActive(false);
    }

    public void ConfirmClearChat()
    {
        photonView.RPC("ClearAllMessagesRPC", RpcTarget.All, PhotonNetwork.NickName);
        clearChatDialog.SetActive(false);
    }

    [PunRPC]
    void ClearAllMessagesRPC(string clearedBy)
    {
        foreach (Transform child in messageContent)
        {
            Destroy(child.gameObject);
        }
        messageQueue.Clear();
        UpdateNoMessageTextVisibility();

        GameObject msgObj = Instantiate(messagePrefab, messageContent);
        msgObj.GetComponent<TMP_Text>().text = $"<i><color=white>{clearedBy} ได้ล้างข้อความแชท</color></i>";
        messageQueue.Enqueue(msgObj);
        scrollRect.verticalNormalizedPosition = 0f;
    }

    // ฟังก์ชันสุ่มสี
    private string GetRandomColorHex()
    {
        Color randomColor = new Color(Random.value, Random.value, Random.value);
        return $"#{ColorUtility.ToHtmlStringRGB(randomColor)}";
    }
}
