using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

using sugi.cc;
using Osc;

public class ProjectionController : SingletonMonoBehaviour<ProjectionController>
{

    GameController gameController { get { return GameController.Instance; } }
    OscController oscController { get { return OscController.Instance; } }
    bool isLeft { get { return GameController.Instance.setting.isLeft; } }

    public Image faderL;
    public Image faderR;
    public VideoPlayer videoL;
    public VideoPlayer videoR;
    Image fader { get
        {
            if (isLeft) return faderL;
            else return faderR;
        }
    }
    VideoPlayer video
    {
        get
        {
            if (isLeft) return videoL;
            else return videoR;
        }
    }

    [Osc("/Title")]
    public void ShowTitle()
    {
        video.Play();
        AudioController.Instance.PlayBGM();
        StartCoroutine(FadeVideoRoutine(1f, 1f));
    }
    IEnumerator FadeVideoRoutine(float to, float duration)
    {
        var t = 0f;
        var videoFrom = video.targetCameraAlpha;
        var faderFrom = fader.color;
        var faderTo = fader.color;
        faderTo.a = to;
        while (t < 1f)
        {
            video.targetCameraAlpha = Mathf.Lerp(videoFrom, to, t);
            fader.color = Color.Lerp(faderFrom, faderTo, t);
            yield return t += Time.deltaTime / duration;
        }
        video.targetCameraAlpha = to;
        fader.color = faderTo;
    }

    [Osc("/Game")]
    public void StartGame()
    {
        StartCoroutine(FadeVideoRoutine(0, 1f));
        gameController.StartGame();
    }

    // Use this for initialization
    void Start()
    {
        video.targetCameraAlpha = 0f;
        oscController.AddCallbacks(this);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
