using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Globalization;

public class YourName : MonoBehaviour
{
    [System.Serializable]
    public class IntroStep
    {
        public Sprite backgroundImage;
        public string storyText;
        public bool isNameInputStep;
        public string buttonText;
    }

    public Image background;
    public TextMeshProUGUI storyText;
    public TMP_InputField nameInputField;
    public Button continueButton;
    public TextMeshProUGUI buttonLabel;
    public TextMeshProUGUI warningText;

    public List<IntroStep> introSteps;
    private int currentStep = 0;
    public string playerName;

    [Header("SFX Sounds")]
    public AudioClip signNameSound;

    private AudioManager audioManager;

    void Start()
{
    nameInputField.gameObject.SetActive(false);
    warningText.gameObject.SetActive(false);

    nameInputField.onValueChanged.AddListener(LimitNameLength);
    nameInputField.onValidateInput += ValidateCharacterInput; // 🔥 ป้องกันพิมพ์อักขระพิเศษ

    ShowStep(currentStep);
    continueButton.onClick.AddListener(OnContinue);

    audioManager = FindObjectOfType<AudioManager>();
}

private char ValidateCharacterInput(string text, int charIndex, char addedChar)
{
    // อนุญาตเฉพาะตัวอักษร, สระ, ตัวเลข, และวรรณยุกต์ไทย
    if (IsValidMainChar(addedChar) || IsThaiToneMark(addedChar))
    {
        return addedChar;
    }

    return '\0'; // ปฏิเสธอักขระนี้
}

    void ShowStep(int step)
    {
        if (step < introSteps.Count)
        {
            background.sprite = introSteps[step].backgroundImage;
            storyText.text = introSteps[step].storyText;
            buttonLabel.text = introSteps[step].buttonText;

            nameInputField.text = "";
            nameInputField.gameObject.SetActive(introSteps[step].isNameInputStep);
        }
    }

   void LimitNameLength(string value)
{
    string result = "";
    int count = 0;

    // ใช้ StringInfo เพื่อจัดการ grapheme clusters
    TextElementEnumerator charEnum = StringInfo.GetTextElementEnumerator(value);

    while (charEnum.MoveNext())
    {
        string grapheme = charEnum.GetTextElement();

        char mainChar = grapheme[0];

        if (IsValidMainChar(mainChar) || IsThaiToneMark(mainChar))
        {
            if (IsThaiToneMark(mainChar))
            {
                result += grapheme; // วรรณยุกต์ไม่ถูกนับ
            }
            else
            {
                if (count < 8)
                {
                    result += grapheme;
                    count++;
                }
                else
                {
                    break;
                }
            }
        }
    }

    nameInputField.text = result;
}

// วรรณยุกต์ไทย (เช่น ่ ้ ๊ ๋ ์ ฯลฯ) – ไม่ถูกนับ
bool IsThaiToneMark(char c)
{
    return (c >= '\u0E47' && c <= '\u0E4E') || c == '\u0E31'; // ครอบคลุมวรรณยุกต์ทั่วไป
}

// ตัวอักษรไทย สระไทย ตัวเลข (ถูกนับ)
bool IsValidMainChar(char c)
{
    // ตัวอักษรอังกฤษ/เลข
    if (char.IsLetterOrDigit(c)) return true;

    // ตัวอักษรไทย (ก-ฮ)
    if (c >= '\u0E01' && c <= '\u0E2E') return true;

    // สระไทย (า ำ เ แ โ ใ ไ ฯลฯ)
    if ((c >= '\u0E30' && c <= '\u0E3A') || (c >= '\u0E40' && c <= '\u0E44')) return true;

    // ไม่ให้พิมพ์อักขระพิเศษได้
    return false;
}

    IEnumerator ShowWarningForTime(string message, float duration)
    {
        warningText.text = message;
        warningText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        warningText.gameObject.SetActive(false);
    }

    void OnContinue()
    {
        if (introSteps[currentStep].isNameInputStep)
        {
            string inputName = nameInputField.text.Trim();

            if (string.IsNullOrEmpty(inputName))
            {
                StartCoroutine(ShowWarningForTime("กรุณาใส่ชื่อของคุณ!!", 2f));
                return;
            }

            if (ContainsProfanity(inputName))
            {
                StartCoroutine(ShowWarningForTime("ชื่อนี้ไม่เหมาะสม!!", 2f));
                return;
            }

            if (HasExcessiveRepeats(inputName))
            {
                StartCoroutine(ShowWarningForTime("ห้ามใส่ตัวอักษรหรือ\nตัวเลขซ้ำเกิน 4 ตัว!!", 2f));
                return;
            }

            playerName = inputName;
        }

        currentStep++;
        if (currentStep < introSteps.Count)
        {
            ShowStep(currentStep);
        }
        else
        {
            audioManager.PlaySFX(signNameSound);
            EndIntro();
        }
    }

    bool HasExcessiveRepeats(string name)
{
    Dictionary<char, int> charCount = new Dictionary<char, int>();

    // Unicode range ของวรรณยุกต์ไทย (U+0E31, U+0E34 - U+0E3A, U+0E47 - U+0E4E)
    string filtered = "";
    foreach (char c in name)
    {
        // กรองวรรณยุกต์ออก
        if (!(c >= '\u0E31' && c <= '\u0E4E'))
        {
            filtered += c;
        }
    }

    foreach (char c in filtered)
    {
        if (!charCount.ContainsKey(c))
            charCount[c] = 1;
        else
            charCount[c]++;

        if (charCount[c] > 4)
            return true;
    }

    return false;
}

    bool ContainsProfanity(string name)
{
    string[] badWords = {
        // English
        "fuck", "shit", "bitch", "asshole", "porn", "sex", "nude", "boob", "dick", "vagina", "penis", "cock", "cum", "tits",
        "slut", "whore", "pussy", "fag", "fucker", "nigga", "nigger", "sexy", "blowjob", "suck", "jerk", "orgasm", "cumming",
        "hentai", "milf", "xxx", "69", "rape", "thot", "bang", "ass", "moan", "titty", "titties",

        // Thai (ทั่วไป)
        "ควย", "เหี้ย", "เย็ด", "หี", "สัส", "สัด", "พ่อมึง", "แม่มึง", "ไอ้เหี้ย", "ฟัค", "ซั่ม", "แตด", "ขี้", "ฆ่าตัวตาย",
        "กระแทก", "จัญไร", "อีดอก", "มึง", "กู", "ชิบหาย", "ดอกทอง", "ล่อ", "เด้า", "อม", "เสียว", "ข่มขืน", "ซิง", "หมอย",
        "ตูด", "ตูดหมึก", "ล่อแม่", "ล่อพ่อ", "ล่อเมีย", "หำ", "เยด", "เย้ด", "หีย์", "หี๋", "เงี่ยน", "ควาย", "ล่อกัน",

        // Thai (แปลงคำ)
        "kuy", "kuay", "hee", "เห้", "ฟัคยู", "ฟัคค", "ฟักยู", "fu*k", "fuq", "fuxk", "sh1t", "b1tch", "p0rn", "s3x", "เซ็กส์", "ซั๊ม",

        // Thai (พิมพ์หลบ เช่น แยกสระ)
        "เ-หี้ย", "ค-ว-ย", "ห-ี", "เ-ย็ด", "แ-ตด", "พ-่อมึง", "แ-ม่มึง"
    };

    string loweredName = name.ToLower();

    foreach (string word in badWords)
    {
        if (loweredName.Contains(word.Replace("-", "")))
        {
            return true;
        }
    }

    return false;
}

    void EndIntro()
    {
        Photon.Pun.PhotonNetwork.NickName = playerName;
        Invoke("LoadNextScene", 0.5f);
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("JoinGameScene");
    }
}
