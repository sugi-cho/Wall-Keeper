using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using sugi.cc;

public class TextPositionSetter : SingletonMonoBehaviour<TextPositionSetter>
{

    public static Vector3 CenterPos
    {
        get
        {
            return GameController.Instance.setting.isLeft ?
                Instance.centerPosL.position : Instance.centerPosR.position;
        }
    }

    public Transform centerPosL;
    public Transform centerPosR;
}
