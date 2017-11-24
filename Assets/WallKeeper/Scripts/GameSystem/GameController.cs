using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using sugi.cc;

public class GameController : SingletonMonoBehaviour<GameController> {
    [Header("set kinect")]
    public GameObject leftKinect;
    public GameObject rightKinect;
    public Setting setting;
    

    private void Awake()
    {
        SettingManager.AddSettingMenu(setting, "GameSetting.json");
        if (setting.isLeft)
            rightKinect.SetActive(false);
        else
            leftKinect.SetActive(false);
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
