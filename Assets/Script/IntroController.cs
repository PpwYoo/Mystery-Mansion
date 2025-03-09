using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class IntroController : MonoBehaviour
{
    public VideoPlayer intro1Player;
    public VideoPlayer intro2Player;
    public Image fadeImage; // Image ที่จะใช้สำหรับ fade effect
    public float fadeDuration = 1f;

    private void Start()
    {
        intro1Player.loopPointReached += OnIntro1End; // เมื่อ Intro1 จบ
        intro2Player.gameObject.SetActive(false); // เริ่มต้น Intro2 ไว้ไม่ให้แสดง
        fadeImage.color = new Color(0, 0, 0, 0); // เริ่มต้น Fade เป็นโปร่งใส
    }

    private void OnIntro1End(VideoPlayer vp)
    {
        StartCoroutine(FadeToBlackAndPlayIntro2());
    }

    private IEnumerator FadeToBlackAndPlayIntro2()
    {
        // Fade to black
        yield return StartCoroutine(Fade(1f));
        
        // หลังจาก fade เสร็จให้เล่น Intro2
        intro2Player.gameObject.SetActive(true);
        intro2Player.Play();

        // Fade จากดำเป็นโปร่งใส
        yield return StartCoroutine(Fade(0f));
        
        // รอจนกว่า Intro2 จะจบ
        intro2Player.loopPointReached += OnIntro2End;
    }

    private void OnIntro2End(VideoPlayer vp)
    {
        StartCoroutine(FadeToBlackAndLoadScene());
    }

    private IEnumerator FadeToBlackAndLoadScene()
    {
        // Fade to black ก่อนเปลี่ยน Scene
        yield return StartCoroutine(Fade(1f));

        // เมื่อ fade เสร็จ ให้โหลด NameScene
        SceneManager.LoadScene("NameScene");
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeImage.color.a;
        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, targetAlpha);
    }
}
