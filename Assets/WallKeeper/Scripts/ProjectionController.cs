using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

using sugi.cc;
using Osc;

public class ProjectionController : SingletonMonoBehaviour<ProjectionController>
{
    public static Color MyColor { get { return isLeft ? Instance.colorL : Instance.colorR; } }
    static bool isLeft { get { return GameController.Instance.setting.isLeft; } }

    GameController gameController { get { return GameController.Instance; } }
    OscController oscController { get { return OscController.Instance; } }

    public Image faderL;
    public Image faderR;
    public VideoPlayer videoL;
    public VideoPlayer videoR;
    public Color colorL = Color.blue;
    public Color colorR = Color.red;
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

    bool started;

    [Osc("/start")]
    public void ShowTitle(object[] data = null)
    {
        if (started)
            return;

        video.Play();
        AudioController.Instance.PlayBGM();
        StartCoroutine(FadeVideoRoutine(1f, 1f));
        this.Invoke(StartGame, 9f);
        started = true;
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
    
    void StartGame()
    {
        StartCoroutine(FadeVideoRoutine(0, 1f, () => video.gameObject.SetActive(false)));
        gameController.StartGame();
    }

    [Osc("/setScore")]
    public void SetScore(object[] data)
    {
        var stageIdx = (int)data[0];
        var score = (int)data[1];
        gameController.SetScore(false, stageIdx, score);
    }
    [Osc("/transfer")]
    public void TransferBall(object[] data)
    {
        var pos = new Vector3((float)data[0], (float)data[1], (float)data[2]);
        var vel = new Vector3((float)data[3], (float)data[4], (float)data[5]);
        var hitCount = (int)data[6];
        var isLeft = (int)data[7] == 1;
        BallController.Instance.TransferBall(pos, vel, hitCount, isLeft);
    }

    // Use this for initialization
    void Start()
    {
        video.targetCameraAlpha = 0f;
        video.Play();
        video.Pause();
        oscController.AddCallbacks(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShowTitle();
            SendStartOsc();
        }
    }

    void SendStartOsc()
    {
        var osc = new MessageEncoder("/start");
        OscController.Instance.Send(osc);
    }
}
