using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

    private string localPlayerColorHex;
    private bool hasStartedChatting = false;

    private const string CHAT_HISTORY_KEY = "chatHistory";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        chatPanel.SetActive(false);
        chatNotificationIcon.SetActive(false);
        noMessageText.SetActive(true);
        UpdateNotificationText();

        // สุ่มสีให้กับชื่อผู้เล่นตัวเอง (ยกเว้นสีขาว)
        localPlayerColorHex = GenerateNonWhiteColorHex();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnJoinedRoom()
    {
        LoadChatHistoryFromRoomProperties();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        chatPanel.SetActive(false);
    }

    void Update()
    {
        // ตรวจจับ Enter เฉพาะตอน inputField โฟกัส
        if (isChatOpen && inputField.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            OnSendButton();
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

#if UNITY_ANDROID || UNITY_IOS
            inputField.DeactivateInputField(); // ไม่ให้แป้นพิมพ์เด้งเอง
#endif
        }
    }

    [PunRPC]
    void ReceiveMessage(string senderName, string messageText)
    {
        string finalMessage;

        if (senderName == PhotonNetwork.NickName)
        {
            finalMessage = $"<b><color={localPlayerColorHex}>{senderName}</color></b>: {messageText}";
        }
        else
        {
            finalMessage = $"<b><color=white>{senderName}</color></b>: {messageText}";
        }

        GameObject msgObj = Instantiate(messagePrefab, messageContent);
        msgObj.GetComponent<TMP_Text>().text = finalMessage;
        messageQueue.Enqueue(msgObj);

        if (messageQueue.Count > maxMessages)
            Destroy(messageQueue.Dequeue());

        UpdateNoMessageTextVisibility();
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;

        if (!isChatOpen)
        {
            unreadCount++;
            chatNotificationIcon.SetActive(true);
            UpdateNotificationText();
        }

        hasStartedChatting = true;
        noMessageText.SetActive(false);

        SaveMessageToRoomProperties(finalMessage);
    }

    private void SaveMessageToRoomProperties(string newMessage)
    {
        object currentHistoryObj;
        PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(CHAT_HISTORY_KEY, out currentHistoryObj);

        List<string> history = currentHistoryObj != null
            ? new List<string>((string[])currentHistoryObj)
            : new List<string>();

        history.Add(newMessage);

        if (history.Count > maxMessages)
            history.RemoveAt(0);

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            [CHAT_HISTORY_KEY] = history.ToArray()
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    private void LoadChatHistoryFromRoomProperties()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(CHAT_HISTORY_KEY, out object historyObj))
        {
            string[] history = (string[])historyObj;
            foreach (string msg in history)
            {
                GameObject msgObj = Instantiate(messagePrefab, messageContent);
                msgObj.GetComponent<TMP_Text>().text = msg;
                messageQueue.Enqueue(msgObj);
            }

            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
            UpdateNoMessageTextVisibility();
        }
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

#if UNITY_ANDROID || UNITY_IOS
            inputField.DeactivateInputField(); // ไม่ให้แป้นพิมพ์เด้งเอง
#else
            inputField.ActivateInputField();
#endif
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
        noMessageText.SetActive(messageQueue.Count == 0 && !hasStartedChatting);
    }

    private void UpdateNotificationText()
    {
        if (notificationCountText != null)
        {
            notificationCountText.text = unreadCount > 0 ? unreadCount.ToString() : "";
        }
    }

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

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            [CHAT_HISTORY_KEY] = new string[0]
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    private string GenerateNonWhiteColorHex()
    {
        Color randomColor;
        do
        {
            randomColor = new Color(Random.value, Random.value, Random.value);
        } while (randomColor.r > 0.9f && randomColor.g > 0.9f && randomColor.b > 0.9f); // หลีกเลี่ยงสีขาว

        return $"#{ColorUtility.ToHtmlStringRGB(randomColor)}";
    }
}
