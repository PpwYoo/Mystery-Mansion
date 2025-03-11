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

    void OnContinue()
    {
        if (introSteps[currentStep].isNameInputStep)
        {
            if (!string.IsNullOrEmpty(nameInputField.text))
            {
                playerName = nameInputField.text;
                Debug.Log("Player Name: " + playerName);
            }
            else
            {
                Debug.LogWarning("Please enter a name!");
                return;
            }
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
