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
    Image fader
    {
        get
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

    [Osc("/title")]
    public void ShowTitle(object[] data = null)
    {
        video.Play();
        AudioController.Instance.PlayBGM();
        StartCoroutine(FadeVideoRoutine(1f, 1f));
    }
    IEnumerator FadeVideoRoutine(float to, float duration, System.Action callback = null)
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
        if (callback != null)
            callback.Invoke();
    }

    [Osc("/game")]
    public void StartGame(object[] data = null)
    {
        StartCoroutine(FadeVideoRoutine(0, 1f, () => video.gameObject.SetActive(false)));
        gameController.StartGame();
    }

    [Osc("/end")]
    public void EndGame(object[] data = null)
    {
        ResultController.Instance.ShowResult();
        foreach (var b in FindObjectsOfType<Ball>())
            b.gameObject.SetActive(false);
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
        if (Input.GetKeyDown(KeyCode.T))
            SendTitleOsc();
        if (Input.GetKeyDown(KeyCode.G))
            SendGameOsc();
        if (Input.GetKeyDown(KeyCode.B))
            SendEndOSC();
    }

    void SendTitleOsc()
    {
        var osc = new MessageEncoder("/title");
        OscController.Instance.Send(osc);
    }
    void SendGameOsc()
    {
        var osc = new MessageEncoder("/game");
        OscController.Instance.Send(osc);
    }
    void SendEndOSC()
    {
        var osc = new MessageEncoder("/end");
        OscController.Instance.Send(osc);
    }
}
