using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using sugi.cc;

public class ResultController : SingletonMonoBehaviour<ResultController>
{
    ProjectionController controller { get { return ProjectionController.Instance; } }
    GameController.GameAudio gameAudio { get { return gameController.gameAudio; } }
    GameController gameController { get { return GameController.Instance; } }
    bool isLeft { get { return gameController.setting.isLeft; } }

    public GameObject winObj;
    public GameObject loseObj;
    public Transform resultCamPosL;
    public Transform resultCamPosR;
    public Transform lookupWallL;
    public Transform lookupWallR;

    public Transform resultObjPosL;
    public Transform resultObjPosR;
    public Transform wallStackPosL;
    public Transform wallStackPosR;
    public float cameraMoveDuration = 4f;

    IEnumerable<Wall> keptWalls;

    Renderer[] renderers;
    bool win;

    Transform resultCamPos
    {
        get
        {
            if (isLeft) return resultCamPosL;
            else return resultCamPosR;
        }
    }
    Transform lookupWall
    {
        get
        {
            return isLeft ?
                lookupWallL : lookupWallR;
        }
    }
    Vector3 wallStackPos
    {
        get
        {
            return (isLeft ? wallStackPosL : wallStackPosR).position;
        }
    }

    public void ShowResult(bool win, IEnumerable<Wall> walls)
    {
        this.win = win;
        keptWalls = walls;
        StartCoroutine(MoveCamPosRoutine(CountWalls));
        this.CallMethodDelayed(25f, () => { controller.EndVideo(); });
    }

    IEnumerator MoveCamPosRoutine(System.Action action = null)
    {
        yield return new WaitForSeconds(2f);

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

    void CountWalls()
    {
        StartCoroutine(CountWallActionRoutine());
    }
    IEnumerator CountWallActionRoutine()
    {
        StartCoroutine(ShowResultRoutine());
        for (var i = 0; i < keptWalls.Count(); i++)
        {
            StartCoroutine(StackWallRoutine(i));
            yield return new WaitForSeconds(0.1f);
        }
        var t = 0f;
        var cam = gameController.gameCam;
        while (t < 1f)
        {
            var rot = Quaternion.Lerp(resultCamPos.rotation, lookupWall.rotation, t);
            cam.transform.rotation = rot;
            yield return t += Time.deltaTime;
        }
    }
    IEnumerator ShowResultRoutine()
    {
        yield return new WaitForSeconds(8.5f);
        ShowResult();
        yield return new WaitForSeconds(2f);
        if (isLeft)
        {
            gameAudio.Play(gameAudio.winnerIs);
            yield return new WaitForSeconds(1.9f);
            if (win)
                gameAudio.Play(gameAudio.team_blue);
            else
                gameAudio.Play(gameAudio.team_red);
        }
        else
            yield return new WaitForSeconds(1.9f);
        gameController.gameCam.transform.rotation = resultCamPos.rotation;
    }

    IEnumerator StackWallRoutine(int idx)
    {
        var wall = keptWalls.ElementAt(idx);
        var pos0 = wall.transform.position;
        var pos1 = pos0;
        pos1.y = 2f;
        var height = idx / 8;
        var rad = Mathf.PI * 2f / 8f * idx + Mathf.PI * 2f / 24f * height;
        var pos2 = wallStackPos
            + Vector3.right * Mathf.Sin(rad)
            + Vector3.forward * Mathf.Cos(rad)
            + Vector3.up * height * 0.27f;
        var rot0 = wall.transform.rotation;
        var rot1 = Quaternion.Euler(0f, rad * Mathf.Rad2Deg, 0f);

        var chain = new CoonsChain();
        chain.positions = new[] { pos0, pos1, pos2 };
        chain.velocities = new[] { Vector3.up, Vector3.zero, Vector3.zero };
        chain.UpdateCurves();

        var t = 0f;
        while (t < 1f)
        {
            wall.transform.position = chain.Interpolate(t);
            wall.transform.rotation = Quaternion.Lerp(rot0, rot1, t);
            yield return t += Time.deltaTime * 0.5f;
        }
        wall.transform.position = pos2;
        wall.transform.rotation = rot1;

        yield return 0;
    }

    void ShowResult()
    {
        foreach (var w in keptWalls)
        {
            var r = w.gameObject.AddComponent<Rigidbody>();
            r.mass = 0.01f;
            r.AddExplosionForce(10f, wallStackPos, 3f);
        }
        var obj = win ? winObj : loseObj;
        var pos = TextPositionSetter.CenterPos;
        obj.transform.position = pos;
        foreach (var r in obj.GetComponentsInChildren<Renderer>())
            r.enabled = true;
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
}

public static class TransformExtention
{
    public static void SetPos(this Transform target, Transform toPos)
    {
        target.SetPositionAndRotation(toPos.position, toPos.rotation);
    }
}