using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class IntroController : MonoBehaviour
{
    public GameObject intro1Object;
    public GameObject intro2Object;
    public GameObject fadePanel;
    public float intro1Duration = 6f;
    public float intro2Duration = 6f;
    public float fadeDuration = 1f;

    private CanvasGroup fadeCanvasGroup;

    void Start()
    {
        fadeCanvasGroup = fadePanel.GetComponent<CanvasGroup>();
        fadePanel.SetActive(false);
        intro1Object.SetActive(true);
        intro2Object.SetActive(false);
        Invoke(nameof(FadeToIntro2), intro1Duration);
    }

    void FadeToIntro2()
    {
        StartCoroutine(FadeTransition(() =>
        {
            intro1Object.SetActive(false);
            intro2Object.SetActive(true);
            Invoke(nameof(FadeOutAfterIntro2), intro2Duration);
        }));
    }

    void FadeOutAfterIntro2()
    {
        StartCoroutine(FadeTransition(LoadNextScene));
    }

    IEnumerator FadeTransition(System.Action onFadeComplete)
    {
        fadePanel.SetActive(true);
        fadeCanvasGroup.alpha = 0f;

        // Fade in
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        onFadeComplete?.Invoke();

        // Fade out
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        fadePanel.SetActive(false);
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("NameScene");
    }
}
