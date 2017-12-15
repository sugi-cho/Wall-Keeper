using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayTimer : MonoBehaviour {

    [Header("image for timer")]
    public Image timerBg0;
    public Image timerBg1;
    public Image timerCircle;
    public float bgAlpha = 0.5f;
    public float circleAlpha = 0.75f;

    [Space(16)]
    public float stageDuration;
    public float currentTime;

    float timeStarted;
    float currentRate { get { return currentTime / stageDuration; } }
    bool isPlaying;

    public void StartStage(float duration)
    {
        stageDuration = duration;
        currentTime = 0f;
        timeStarted = Time.timeSinceLevelLoad;
        isPlaying = true;
        Show();
    }
    void Show()
    {
        StartCoroutine(ShowRoutine());
    }
    IEnumerator ShowRoutine()
    {
        var t = 0f;
        var col0 = timerBg0.color;
        var col1 = timerBg1.color;
        var col2 = timerCircle.color;
        var rct0 = timerBg0.rectTransform;
        var rct1 = timerBg1.rectTransform;
        var rct2 = timerCircle.rectTransform;
        while(t < 1f)
        {
            {
                col0.a = col1.a = Mathf.Lerp(0f, bgAlpha, t);
                col2.a = Mathf.Lerp(0f, circleAlpha, t);
                timerBg0.color = col0;
                timerBg1.color = col1;
                timerCircle.color = col2;
            }
            {
                rct0.localScale = Vector3.one * Mathf.Sqrt(t);
                rct1.localScale = Vector3.one * t * t;
                rct2.localScale = Vector3.one * t;
            }
            yield return t += Time.deltaTime;
        }
        col0.a = col1.a = bgAlpha;
        col2.a = circleAlpha;
        timerBg0.color = col0;
        timerBg1.color = col1;
        timerCircle.color = col2;
        rct0.localScale = rct2.localScale = rct1.localScale = Vector3.one;
    }
    public void Hide(float duration = 1f)
    {
        StartCoroutine(HideRoutine(duration));
    }
    IEnumerator HideRoutine(float duration)
    {
        var col0 = timerBg0.color;
        var col1 = timerBg1.color;
        var col2 = timerCircle.color;
        var t = 0f;
        while(t < 1f)
        {
            col0.a = col1.a = Mathf.Lerp(bgAlpha, 0f, t);
            col2.a = Mathf.Lerp(circleAlpha, 0f, t);
            timerBg0.color = col0;
            timerBg1.color = col1;
            timerCircle.color = col2;
            yield return t += Time.deltaTime / duration;
        }
        col0.a = col1.a = col2.a = 0f;
        timerBg0.color = col0;
        timerBg1.color = col1;
        timerCircle.color = col2;
    }

    private void Start()
    {
        Hide(0);
    }
    // Update is called once per frame
    void Update () {
        if (isPlaying)
        {
            currentTime = Time.timeSinceLevelLoad - timeStarted;
            timerCircle.fillAmount = 1f - currentRate;
        }
        else
            timerCircle.fillAmount = 1f;
	}
}
