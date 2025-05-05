using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Globalization;
using System.Collections;
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
    public Image duplicateWarningImage;
    public TMP_Text dateText;
    private string lastMessageDate = "";

    [Header("Scroll Down Button")]
    public GameObject scrollDownButton;
    private CanvasGroup scrollButtonCanvasGroup;
    private Coroutine fadeCoroutine;
    private bool isScrollButtonVisible = false;

    [Header("Clear Chat Confirmation")]
    public GameObject clearChatDialog;

    private Queue<GameObject> messageQueue = new Queue<GameObject>();
    private int maxMessages = 100;
    private bool isChatOpen = false;
    private int unreadCount = 0;
    private bool hasSystemMessageUnread = false;
    private bool hasStartedChatting = false;

    private string localPlayerColorHex;
    private const string CHAT_HISTORY_KEY = "chatHistory";

    private Coroutine duplicateWarningCoroutine;

    private bool isUserScrollingManually = false;

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

        localPlayerColorHex = GenerateNonWhiteColorHex();
        duplicateWarningImage.gameObject.SetActive(false);

        scrollButtonCanvasGroup = scrollDownButton.GetComponent<CanvasGroup>();
        scrollButtonCanvasGroup.alpha = 0;
        scrollDownButton.SetActive(true);

        UpdateDate();

        scrollRect.onValueChanged.AddListener(OnScrollValueChanged);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        chatPanel.SetActive(false);
    }

    void FadeInScrollButton()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(scrollButtonCanvasGroup, 0f, 1f, 0.3f));
        isScrollButtonVisible = true;

        // Play sound for scroll-down button appearance
        SFXManager.Instance?.PlayArrowButton();  // <-- Added this line for sound effect (appearance)
    }

    void FadeOutScrollButton()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(scrollButtonCanvasGroup, 1f, 0f, 0.3f));
        isScrollButtonVisible = false;

        SFXManager.Instance?.PlayArrowButton();  // <-- Added this line for sound effect (disappearance)
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        cg.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        cg.alpha = to;
    }

    void Update()
    {
        if (isChatOpen && inputField.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            OnSendButton();
        }

        if (isChatOpen && inputField.isFocused && Input.anyKeyDown)
        {
            SFXManager.Instance?.PlayTyping();
        }

        if (scrollRect.content.sizeDelta.y > scrollRect.viewport.rect.height)
        {
            bool isAtBottom = scrollRect.verticalNormalizedPosition <= 0.0001f;
            if (!isAtBottom && !isScrollButtonVisible)
            {
                FadeInScrollButton();
            }
            else if (isAtBottom && isScrollButtonVisible)
            {
                FadeOutScrollButton();
            }
        }
        else if (isScrollButtonVisible)
        {
            FadeOutScrollButton();
        }

        if (System.DateTime.Now.ToString("yyyyMMdd") != lastMessageDate)
        {
            UpdateDate(); // อัปเดตวันที่ใหม่
        }
    }

    void UpdateDate()
    {
        string today = System.DateTime.Now.ToString("dd MMM yyyy", new CultureInfo("en-US")).ToUpper();
    
    // Check if the date has changed
    if (today != lastMessageDate)
    {
        dateText.text = today; // Update the date if it's a new day
        lastMessageDate = today; // Save the current date as the last updated date
    }
    }

    public void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
        isUserScrollingManually = false;
        FadeOutScrollButton();

        // Play sound when scrolling to the bottom
        SFXManager.Instance?.PlayArrowButton();  // <-- Added this line for sound effect
    }

    void OnScrollValueChanged(Vector2 pos)
    {
        bool isAtBottom = scrollRect.verticalNormalizedPosition <= 0.01f;
        isUserScrollingManually = !isAtBottom;
    }

    public override void OnJoinedRoom()
    {
        LoadChatHistoryFromRoomProperties();
    }

    public void ToggleChatPanel()
    {
        isChatOpen = !isChatOpen;
        chatPanel.SetActive(isChatOpen);

        if (isChatOpen)
        {
            SFXManager.Instance?.PlayChatOpen();
            chatNotificationIcon.SetActive(false);
            unreadCount = 0;
            hasSystemMessageUnread = false;
            UpdateNotificationText();

#if UNITY_ANDROID || UNITY_IOS
            inputField.DeactivateInputField();
#else
            inputField.ActivateInputField();
#endif
            UpdateNoMessageTextVisibility();
        }
        else
        {
            SFXManager.Instance?.PlayChatClose();
        }
    }

    public bool isChatPanelOpen()
    {
        return chatPanel.activeSelf;
    }

    public void OnSendButton()
    {
        string rawText = inputField.text;

        if (!string.IsNullOrWhiteSpace(rawText) && CountThaiBaseCharacters(rawText) > 0)
        {
            if (HasExcessiveDuplicateCharacters(rawText, 10))
            {
                ShowDuplicateWarningImage();
                SFXManager.Instance?.PlayNotification();
                return;
            }

            photonView.RPC("ReceiveMessage", RpcTarget.All, PhotonNetwork.NickName, rawText);
            SFXManager.Instance?.PlaySendMessage();
            inputField.text = "";

#if UNITY_ANDROID || UNITY_IOS
            inputField.DeactivateInputField();
#endif
        }
    }

    [PunRPC]
void ReceiveMessage(string senderName, string messageText)
{
    string senderColorHex;

    if (senderName == PhotonNetwork.NickName)
    {
        senderColorHex = localPlayerColorHex;
    }
    else
    {
        senderColorHex = "#000000";
    }

    string timeStamp = System.DateTime.Now.ToString("HH:mm");

    string finalMessage = $"<b><color={senderColorHex}>{senderName}:</color></b> {messageText} <size=70%><color=#666666>({timeStamp})</color></size>";

    GameObject msgObj = Instantiate(messagePrefab, messageContent);
    msgObj.GetComponent<TMP_Text>().text = finalMessage;
    messageQueue.Enqueue(msgObj);

    if (messageQueue.Count > maxMessages)
        Destroy(messageQueue.Dequeue());

    UpdateNoMessageTextVisibility();
    Canvas.ForceUpdateCanvases();

    if (!isUserScrollingManually)
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
        if (!PhotonNetwork.InRoom) return;
        PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(CHAT_HISTORY_KEY, out object currentHistoryObj);

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

    public void ShowClearChatConfirmation()
    {
        SFXManager.Instance?.PlayClearChat();
        clearChatDialog.SetActive(true);
    }

    public void CancelClearChat()
    {
        SFXManager.Instance?.PlayCancelClear();
        clearChatDialog.SetActive(false);
    }

    public void ConfirmClearChat()
    {
        SFXManager.Instance?.PlayConfirmClear();
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
        msgObj.GetComponent<TMP_Text>().text = $"<i><color=#666666>\"{clearedBy}\" ได้ล้างข้อความแชท</color></i>";
        messageQueue.Enqueue(msgObj);
        scrollRect.verticalNormalizedPosition = 0f;

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            [CHAT_HISTORY_KEY] = new string[0]
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        if (!isChatOpen)
        {
            hasSystemMessageUnread = true;
            chatNotificationIcon.SetActive(true);
            UpdateNotificationText();
        }
    }

    private void ShowDuplicateWarningImage()
    {
        if (duplicateWarningCoroutine != null)
            StopCoroutine(duplicateWarningCoroutine);

        duplicateWarningCoroutine = StartCoroutine(ShowDuplicateImageRoutine());
    }

    private IEnumerator ShowDuplicateImageRoutine()
    {
        duplicateWarningImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        duplicateWarningImage.gameObject.SetActive(false);
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
                count++;
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
                    return true;
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
            if (hasSystemMessageUnread)
                notificationCountText.text = "!";
            else
                notificationCountText.text = unreadCount > 0 ? unreadCount.ToString() : "";
        }
    }

    private string GenerateNonWhiteColorHex()
    {
        Color randomColor;
        do
        {
            randomColor = new Color(Random.value, Random.value, Random.value);
        } while (randomColor.r > 0.9f && randomColor.g > 0.9f && randomColor.b > 0.9f);

        return $"#{ColorUtility.ToHtmlStringRGB(randomColor)}";
    }
}
