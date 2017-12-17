using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using sugi.cc;
using Osc;

public class AudioController : SingletonMonoBehaviour<AudioController>
{
    bool IsLeft { get { return GameController.Instance.setting.isLeft; } }

    public AudioClip bgm;

    public AudioClip bodyHit;
    public AudioClip wallHit;
    public AudioClip wallBreak;
    public AudioClip countFive;
    public AudioClip countTen;
    public AudioClip countWalls;
    public AudioClip wallAppear;
    public AudioClip passThrough;

    public Setting setting;

    public int seLimit = 8;
    List<AudioSource> sePlayersL;
    List<AudioSource> sePlayersR;

    AudioSource bgmSource;

    public void OnBodyHit(bool isLeft = true)
    {
        if (IsLeft)
            PlaySE(bodyHit, isLeft);
        else
            PlayRemoteSE("bodyHit");
    }
    public void OnWallHit(bool isLeft = true)
    {
        if (IsLeft)
            PlaySE(wallHit, IsLeft);
        else
            PlayRemoteSE("wallHit");
    }
    public void OnWallBreak(bool isLeft = true)
    {
        if (IsLeft)
            PlaySE(wallBreak);
        else
            PlayRemoteSE("wallBreak");
    }
    public void OnPassThrogh()
    {
        if (IsLeft)
            AudioSource.PlayClipAtPoint(passThrough, Vector3.zero);
        else
            PlayRemoteSE("passThrough");
    }
    public void OnCountFive()
    {
        if (IsLeft)
            AudioSource.PlayClipAtPoint(countFive, Vector3.zero);
    }
    public void OnCountTen()
    {
        if (IsLeft)
            AudioSource.PlayClipAtPoint(countTen, Vector3.zero);
    }

    void PlayRemoteSE(string seName)
    {
        var osc = new MessageEncoder("/se/" + seName);
        OscController.Instance.Send(osc);
    }
    public void PlaySE(AudioClip clip, bool isLeft = true)
    {
        if (clip == null)
            return;
        var players = isLeft ? sePlayersL : sePlayersR;
        var player = players.Where(ap => !ap.isPlaying).FirstOrDefault();
        if (player != null)
        {
            player.clip = clip;
            player.Play();
        }
    }

    public void PlayBGM()
    {
        if (bgm != null && setting.isBgmSource)
            bgmSource.Play();
    }

    // Use this for initialization
    void Start()
    {
        SettingManager.AddSettingMenu(setting, "audioSetting.json");

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0f;
        bgmSource.panStereo = 1f;
        bgmSource.volume = setting.bgmVolume;
        bgmSource.clip = bgm;

        sePlayersL = new List<AudioSource>();
        for (var i = 0; i < seLimit; i++)
        {
            var audio = gameObject.AddComponent<AudioSource>();
            audio.panStereo = -1f;
            audio.spatialBlend = 0f;
            audio.playOnAwake = false;
            audio.volume = setting.seVolume;
            sePlayersL.Add(audio);
        }

        sePlayersR = new List<AudioSource>();
        for (var i = 0; i < seLimit; i++)
        {
            var audio = gameObject.AddComponent<AudioSource>();
            audio.panStereo = 1f;
            audio.spatialBlend = 0f;
            audio.playOnAwake = false;
            audio.volume = setting.seVolume;
            sePlayersR.Add(audio);
        }
    }

    [System.Serializable]
    public class Setting : SettingManager.Setting
    {
        public bool isBgmSource;
        public float bgmVolume = 1f;
        public float seVolume = 1f;
    }
}
