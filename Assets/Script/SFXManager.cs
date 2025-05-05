using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("SFX Clips")]
    public AudioClip openChatClip;
    public AudioClip closeChatClip;
    public AudioClip sendMessageClip;
    public AudioClip clearChatClip;
    public AudioClip notificationClip;
    public AudioClip typingClip;
    public AudioClip messageReceivedClip;
    public AudioClip receiveMessageClip;  // เสียงเมื่อรับข้อความ
    public AudioClip confirmClearChatClip; // เสียงเมื่อกดยืนยันล้างแชท
    public AudioClip cancelClearChatClip;  // เสียงเมื่อกดยกเลิกล้างแชท
    public AudioClip scrollDownClip;  // เสียงสำหรับปุ่ม Scroll Down
    public AudioClip arrowButtonClip;

    private AudioSource audioSource;

    public enum SoundType
    {
        OpenChat,
        CloseChat,
        SendMessage,
        ClearChat,
        Notification,
        Typing,
        MessageReceived,
        ConfirmClearChat,
        CancelClearChat,
        ReceiveMessage,
        ScrollDown
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    // Play sound based on SoundType
    public void PlaySound(SoundType soundType)
    {
        switch (soundType)
        {
            case SoundType.OpenChat:
                PlaySound(openChatClip);
                break;
            case SoundType.CloseChat:
                PlaySound(closeChatClip);
                break;
            case SoundType.SendMessage:
                PlaySound(sendMessageClip);
                break;
            case SoundType.ClearChat:
                PlaySound(clearChatClip);
                break;
            case SoundType.Notification:
                PlaySound(notificationClip);
                break;
            case SoundType.Typing:
                PlaySound(typingClip);
                break;
            case SoundType.MessageReceived:
                PlaySound(messageReceivedClip);
                break;
            case SoundType.ConfirmClearChat:
                PlaySound(confirmClearChatClip);
                break;
            case SoundType.CancelClearChat:
                PlaySound(cancelClearChatClip);
                break;
            case SoundType.ReceiveMessage:
                PlaySound(receiveMessageClip);
                break;
            case SoundType.ScrollDown:
                PlaySound(scrollDownClip); // เล่นเสียงสำหรับ Scroll Down
                break;
        }
    }

    public void PlayArrowButton()
    {
        if (arrowButtonClip != null)
        {
            audioSource.PlayOneShot(arrowButtonClip);
        }
    }


    // Play specific sound directly
    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // New helper methods to trigger sound effects based on ChatManager events
    public void PlayChatOpen() => PlaySound(SoundType.OpenChat);
    public void PlayChatClose() => PlaySound(SoundType.CloseChat);
    public void PlaySendMessage() => PlaySound(SoundType.SendMessage);
    public void PlayClearChat() => PlaySound(SoundType.ClearChat);
    public void PlayNotification() => PlaySound(SoundType.Notification);
    public void PlayTyping() => PlaySound(SoundType.Typing);
    public void PlayMessageReceived() => PlaySound(SoundType.MessageReceived);
    public void PlayReceiveMessage() => PlaySound(SoundType.ReceiveMessage);
    public void PlayConfirmClear() => PlaySound(SoundType.ConfirmClearChat);
    public void PlayCancelClear() => PlaySound(SoundType.CancelClearChat);
    public void PlayScrollDown() => PlaySound(SoundType.ScrollDown);
}
