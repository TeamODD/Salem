using System.Collections;
using TMPro;
using UnityEngine;

public class IntroManager : MonoBehaviour
{
    public CanvasGroup PanelCanvasGroup;
    public TextMeshProUGUI Text;

    public float TypeSpeed = 0.05f;
    public float DisplayDuration = 2f;
    public float FadeDuration = 1f;

    void Start()
    {
        Text.text = "";
        Time.timeScale = 0f; // 인트로 시작 시 게임 시간 정지
        StartCoroutine(StartIntro());
    }

    IEnumerator StartIntro()
    {
        string infoText = "마을에는 신자, 좀도둑, 겁쟁이가 살고 있다.\n\n" + "이들 중에는 1명의 마녀가 숨어있다."; // 추후 수정 필요

        yield return StartCoroutine(TypeText(infoText));

        yield return new WaitForSecondsRealtime(DisplayDuration);
        Text.text = "";

        float timer = 0f;
        while (timer < FadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            PanelCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / FadeDuration);
            yield return null;
        }

        PanelCanvasGroup.gameObject.SetActive(false);

        Time.timeScale = 1f; // 인트로 종료 시 게임 시간 재개
    }
    IEnumerator TypeText(string message)
    {
        Text.text = "";
        foreach (char letter in message.ToCharArray())
        {
            Text.text += letter;
            yield return new WaitForSecondsRealtime(TypeSpeed);
        }
    }
}