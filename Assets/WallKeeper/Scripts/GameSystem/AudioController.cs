using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using sugi.cc;

public class AudioController : SingletonMonoBehaviour<AudioController> {

    public AudioClip bgm;

    public AudioClip bodyHit;
    public AudioClip wallHit;
    public AudioClip wallBreak;

    AudioSource bgmSource;

    public void OnBodyHit()
    {
        AudioSource.PlayClipAtPoint(bodyHit, Vector3.zero);
    }
    public void OnWallHit()
    {
        AudioSource.PlayClipAtPoint(wallHit, Vector3.zero);
    }
    public void OnWallBreak()
    {
        AudioSource.PlayClipAtPoint(wallBreak, Vector3.zero);
    }

    public void PlayBGM()
    {
        bgmSource.Play();
    }

	// Use this for initialization
	void Start () {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.clip = bgm;
	}
	
}
