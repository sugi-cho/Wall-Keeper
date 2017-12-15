using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using sugi.cc;

public class AudioController : SingletonMonoBehaviour<AudioController>
{

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
    List<AudioSource> sePlayers;

    AudioSource bgmSource;

    public void OnBodyHit()
    {
        PlaySE(bodyHit);
    }
    public void OnWallHit()
    {
        PlaySE(wallHit);
    }
    public void OnWallBreak()
    {
        PlaySE(wallBreak);
    }
    public void OnCountFive()
    {
        PlaySE(countFive);
    }
    public void OnCountTen()
    {
        PlaySE(countTen);
    }

    void PlaySE(AudioClip clip)
    {
        if (clip == null)
            return;
        var player = sePlayers.Where(ap => !ap.isPlaying).FirstOrDefault();
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
        bgmSource.spatialBlend = -1f;
        bgmSource.panStereo = 1f;
        bgmSource.volume = setting.bgmVolume;
        bgmSource.clip = bgm;

        sePlayers = new List<AudioSource>();
        for (var i = 0; i < seLimit; i++)
        {
            var audio = gameObject.AddComponent<AudioSource>();
            audio.panStereo = -1f;
            audio.spatialBlend = 0f;
            audio.playOnAwake = false;
            audio.volume = setting.seVolume;
            sePlayers.Add(audio);
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
