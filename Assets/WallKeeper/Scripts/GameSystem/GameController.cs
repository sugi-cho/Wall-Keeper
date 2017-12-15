using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using sugi.cc;
using Osc;

public class GameController : SingletonMonoBehaviour<GameController>
{
    public Setting setting;

    [Header("cameras")]
    public Camera leftCam;
    public Camera rightCam;

    public KinectController gameKinect { get; private set; }
    public Camera gameCam { get; private set; }
    public BallController ballController { get { return BallController.Instance; } }
    public AudioController audioController { get { return AudioController.Instance; } }

    public Plane gamePlane { get; private set; }
    public bool started { get; private set; }

    [Header("3d texts")]
    public StageTexts stageTexts;
    public CountTexts countTexts;

    public PlayTimer timerL;
    public PlayTimer timerR;
    PlayTimer timer
    {
        get
        {
            if (setting.isLeft)
                return timerL;
            return timerR;
        }
    }

    public GameObject teamObjL;
    public GameObject teamObjR;
    GameObject teamObj
    {
        get
        {
            return setting.isLeft ?
                teamObjL : teamObjR;
        }
    }

    public Transform centerPosL;
    public Transform centerPosR;
    public Transform centerPos
    {
        get
        {
            return setting.isLeft ?
                centerPosL : centerPosR;
        }
    }

    public GameObject floorObj;

    public int remainNumWalls;

    public int[] myScores;
    public int[] oppositeScores;

    public GameObject[] wallsObjs;
    public List<Wall> currentWallList;
    public int breakCount { get; private set; }
    public float maxBallSpeed = 3f;

    int stageCount;
    bool end;

    public void SetScore(bool isMine, int stageIdx, int score)
    {
        if (isMine)
        {
            myScores[stageIdx] = score;
            var osc = new MessageEncoder("/setScore");
            osc.Add(stageIdx);
            osc.Add(score);
            OscController.Instance.Send(osc);
        }
        else
            oppositeScores[stageIdx] = score;

        if (stageIdx == 2)
            if (-1 < myScores[stageIdx] && -1 < oppositeScores[stageIdx])
                GetResult();
    }
    void GetResult()
    {
        if (end) return;
        var myScore = myScores.Sum();
        var oppositeScore = oppositeScores.Sum();
        ResultController.Instance.ShowResult(win: oppositeScore <= myScore);
        end = true;
    }

    public void StartGame()
    {
        stageTexts.transform.position = centerPos.position;
        countTexts.transform.position = centerPos.position;
        myScores = Enumerable.Repeat(-1, 3).ToArray();
        oppositeScores = Enumerable.Repeat(-1, 3).ToArray();
        StartCoroutine(StageRoutine());

        started = true;
    }
    IEnumerator StageRoutine()
    {
        StartCoroutine(ExplainRoutine());
        yield return new WaitForSeconds(10f);

        //FirstStage
        var stageDuration = 20f;
        maxBallSpeed = 1f;
        breakCount = 2;
        StartStage(stageDuration);
        this.Invoke(CountDown, stageDuration - 5f);
        yield return new WaitForSeconds(stageDuration);

        var countDuration = 10f;
        var remainWalls = CountReminWalls();
        ballController.ClearBalls();
        timer.Hide();
        SetScore(true, stageCount, remainWalls);
        yield return new WaitForSeconds(countDuration);

        //SecondStage!!
        stageCount++;
        stageDuration = 30f;
        maxBallSpeed = 5f;
        StartStage(stageDuration);
        this.Invoke(CountDown, stageDuration - 5f);
        yield return new WaitForSeconds(stageDuration);

        countDuration = 10f;
        ballController.ClearBalls();
        timer.Hide();
        remainWalls = CountReminWalls();
        SetScore(true, stageCount, remainWalls);
        yield return new WaitForSeconds(countDuration);

        //FinalStage
        stageCount++;
        stageDuration = 30f;
        maxBallSpeed = 9f;
        StartStage(stageDuration);
        this.CallMethodDelayed(stageDuration - 10f, () => audioController.OnCountTen());
        this.Invoke(CountDown, stageDuration - 5f);
        yield return new WaitForSeconds(stageDuration);

        ballController.ClearBalls();
        timer.Hide();
        remainWalls = CountReminWalls();
        SetScore(true, stageCount, remainWalls);
        //SetScore will call osc send if stageCount == 2

        yield return new WaitForSeconds(1f);
        GetResult();
    }


    IEnumerator ExplainRoutine()
    {
        yield return new WaitForSeconds(2f);
        var t = 0f;
        while (t < 1f)
        {
            yield return t += Time.deltaTime / 3f;
        }
        teamObj.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        BallController.Instance.AddBall(centerPos.position);
    }

    void StartStage(float stageDuration)
    {
        ballController.AddBallWithTimeLine(stageCount);
        StartCoroutine(StartStageRoutine());
        timer.StartStage(stageDuration);
        SetWalls();
    }
    IEnumerator StartStageRoutine()
    {
        stageTexts.ShowTitle(stageCount);
        yield return new WaitForSeconds(2f);
        stageTexts.ShowStart();
        yield return new WaitForSeconds(1f);
        stageTexts.Hide();
    }

    void SetWalls()
    {
        var walls = wallsObjs[stageCount].GetComponentsInChildren<Wall>();

        var side = setting.isLeft ? Wall.Side.Left : Wall.Side.Right;
        currentWallList.Clear();
        currentWallList.AddRange(walls.Where(w => w.side == side));
        foreach (var w in currentWallList)
            w.Show();
    }

    int CountReminWalls()
    {
        var count = currentWallList.Count(w => !w.broken);
        return count;
    }

    void CountDown()
    {
        StartCoroutine(FinishRoutine());
        AudioController.Instance.OnCountFive();
    }
    IEnumerator FinishRoutine()
    {
        countTexts.CountDouwn();
        while (countTexts.isCounting)
            yield return 0;
        stageTexts.ShowFinish();
        yield return new WaitForSeconds(1f);
        stageTexts.Hide();
    }

    private void Awake()
    {
        SettingManager.AddSettingMenu(setting, "GameSetting.json");
        if (setting.isLeft)
        {
            rightCam.enabled = false;
            gameCam = leftCam;
        }
        else
        {
            leftCam.enabled = false;
            gameCam = rightCam;
        }
        gamePlane = new Plane(Vector3.up, ballController.transform.position);
        currentWallList = new List<Wall>();
    }

    [System.Serializable]
    public class Setting : SettingManager.Setting
    {
        public bool isLeft;
    }
}
