using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class IntroStory : MonoBehaviour
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
        ShowStep(currentStep);
        continueButton.onClick.AddListener(OnContinue);

        audioManager = FindObjectOfType<AudioManager>();
    }

    void ShowStep(int step)
    {
        if (step < introSteps.Count)
        {
            background.sprite = introSteps[step].backgroundImage;
            storyText.text = introSteps[step].storyText;
            buttonLabel.text = introSteps[step].buttonText;

            if (introSteps[step].isNameInputStep)
            {
                nameInputField.gameObject.SetActive(true);
            }
            else
            {
                nameInputField.gameObject.SetActive(false);
            }
        }
    }

    IEnumerator ShowWarningForTime(string message, float duration)
{
    warningText.text = message; // แสดงข้อความ
    yield return new WaitForSeconds(duration); // รอ 3 วินาที
    warningText.text = ""; // ซ่อนข้อความ
}

    void OnContinue()
{
    if (introSteps[currentStep].isNameInputStep)
    {
        string inputName = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(inputName))
        {
            StartCoroutine(ShowWarningForTime("กรุณาใส่ชื่อของคุณ!!", 3f));
            return;
        }

        if (inputName.Length > 8)
        {
            StartCoroutine(ShowWarningForTime("ห้ามเกิน 8 ตัวอักษร!!", 3f));
            return;
        }

        if (ContainsProfanity(inputName))
        {
            StartCoroutine(ShowWarningForTime("ชื่อนี้ไม่เหมาะสม!!", 3f));
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

bool ContainsProfanity(string name)
{
    string[] badWords = {
        "fuck", "shit", "bitch", "asshole", "bastard", "dick", "pussy", "cunt", "douche", "bimbo", "twat", "fag", "nigga", "slut", "whore",
        "ass", "bastard", "piss", "douchebag", "scumbag", "cockhead", "shithead", "cocksucker", "whorebag", "skank", "dickhead", "pimp", 
        "fucker", "numbnuts", "sucks", "butthead", "porn", "xxx", "sex", "nude", "boob", "tit", "ass", "clit", "cock", "vagina", "blowjob", 
        "masturbate", "orgy", "cum", "penis", "naked", "pornhub", "hentai", "suck", "breast", "suicide", "hitler", "racist", "yed", "hee", "kuy", 
        "gay", "kuay", "booba", "boobs", "fap", "sexy", "breast", "tits", "boobs", "thrust", "fuckface", "lick", "fucking", "orgasm", "fucktard", 
        "cumshot", "pussyhole", "sucking", "pussys", "sexting", "climax", "vulva", "bimbo", "tramp", "escort", "callgirl", "hooker", "whoring", 
        "cumbucket", "sexslave", "fap", "slutty", "wetdream", "rape", "porns", "breasts", "bigboob", "bigboobs", "hugeboob", "megaboob", "bigtit", 
        "bigtits", "bigkuay", "bigkuy", "heeyai", "heelek", "hugetit", "hugetits", "bigbooba", "kuayyai", "kuaylek", "kuyyai", "kuylek", "hitlers"
    };

    string loweredName = name.ToLower();
    foreach (string word in badWords)
    {
        if (loweredName.Contains(word))
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
