using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using sugi.cc;

public class ResultController : SingletonMonoBehaviour<ResultController>
{

    GameController gameController { get { return GameController.Instance; } }
    bool isLeft { get { return gameController.setting.isLeft; } }

    public GameObject winObj;
    public GameObject loseObj;
    public Transform resultCamPosL;
    public Transform resultCamPosR;
    public Transform resultObjPosL;
    public Transform resultObjPosR;
    public float cameraMoveDuration = 4f;

    Renderer[] renderers;

    Transform resultCamPos
    {
        get
        {
            if (isLeft) return resultCamPosL;
            else return resultCamPosR;
        }
    }

    public void ShowResult(bool win)
    {
        var obj = win ? winObj : loseObj;
        var pos = TextPositionSetter.CenterPos;
        StartCoroutine(MoveCamPosRoutine(
            () =>
            {
                obj.transform.position = pos;
                foreach (var r in obj.GetComponentsInChildren<Renderer>())
                    r.enabled = true;
            }
            ));
    }

    IEnumerator MoveCamPosRoutine(System.Action action = null)
    {
        var cam = gameController.gameCam;
        var fromPos = cam.transform.position;
        var fromRot = cam.transform.rotation;
        var toPos = resultCamPos.transform.position;
        var toRot = resultCamPos.transform.rotation;

        var t = 0f;
        while (t < 1f)
        {
            cam.transform.position = Vector3.Lerp(fromPos, toPos, t);
            cam.transform.rotation = Quaternion.Lerp(fromRot, toRot, t);
            yield return t += Time.deltaTime / cameraMoveDuration;
        }
        cam.transform.position = toPos;
        cam.transform.rotation = toRot;

        if (action != null)
            action.Invoke();
    }

    // Use this for initialization
    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            r.SetColor("_Color", ProjectionController.MyColor);
            r.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

public static class TransformExtention
{
    public static void SetPos(this Transform target, Transform toPos)
    {
        target.SetPositionAndRotation(toPos.position, toPos.rotation);
    }
}