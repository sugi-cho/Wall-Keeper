using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using sugi.cc;

public class GameController : SingletonMonoBehaviour<GameController> {
    [Header("set kinect")]
    public GameObject leftKinect;
    public GameObject rightKinect;
    public Setting setting;

    [Header("cameras")]
    public Camera leftCam;
    public Camera rightCam;

    public KinectController gameKinect { get; private set; }
    public Camera gameCam { get; private set; }

    private void Awake()
    {
        SettingManager.AddSettingMenu(setting, "GameSetting.json");
        if (setting.isLeft)
        {
            rightKinect.SetActive(false);
            rightCam.enabled = false;

            gameKinect = leftKinect.GetComponent<KinectController>();
            gameCam = leftCam;
        }
        else
        {
            leftKinect.SetActive(false);
            leftCam.enabled = false;

            gameKinect = rightKinect.GetComponent<KinectController>();
            gameCam = rightCam;
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    [System.Serializable]
    public class Setting : SettingManager.Setting
    {
        public bool isLeft;
    }
}
