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

    public Material kinectVis;
    public Fader fader;

    public GameObject floorObj;
    Renderer[] floorRenderers;

    public int remainNumWalls;

    public int[] myScores;
    public int[] oppositeScores;

    public GameObject[] wallsObjs;
    public List<Wall> currentWallList;
    public int breakCount { get; private set; }
    public float maxBallSpeed = 3f;

    public GameAudio gameAudio;

    public AudioSource wallCountSound;

    List<Wall> keptWallList = new List<Wall>();

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
        ResultController.Instance.ShowResult(win: oppositeScore <= myScore, walls: keptWallList);
        end = true;
    }

    public void StartGame()
    {
        kinectVis.SetFloat("_Amount", 0);
        stageTexts.transform.position = centerPos.position;
        countTexts.transform.position = centerPos.position;
        myScores = Enumerable.Repeat(-1, 3).ToArray();
        oppositeScores = Enumerable.Repeat(-1, 3).ToArray();
        StartCoroutine(StageRoutine());
        floorRenderers = floorObj.GetComponentsInChildren<Renderer>();
        foreach (var r in floorRenderers)
            r.SetFloat("_T", 0f);

        started = true;
        fader.Fade(0f);
    }
    IEnumerator StageRoutine()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(ExplainRoutine());
        yield return new WaitForSeconds(10f);

        //FirstStage
        var stageDuration = 20f;
        //stageDuration = 10f;

        maxBallSpeed = 3f;
        breakCount = 2;
        gameAudio.Play(gameAudio.firstStage);
        StartStage(stageDuration);
        this.Invoke(CountDown, stageDuration - 5f);
        yield return new WaitForSeconds(stageDuration);

        var countDuration = 10f;
        var remainWalls = CountRemainWalls();
        ballController.ClearBalls();
        timer.Hide();
        SetScore(true, stageCount, remainWalls);
        yield return new WaitForSeconds(countDuration);

        //SecondStage!!
        stageCount++;
        stageDuration = 30f;
        //stageDuration = 10f;

        maxBallSpeed = 7f;
        gameAudio.Play(gameAudio.secondStage);
        StartStage(stageDuration);
        SetWalls();
        this.Invoke(CountDown, stageDuration - 5f);
        yield return new WaitForSeconds(stageDuration);

        countDuration = 10f;
        ballController.ClearBalls();
        timer.Hide();
        remainWalls = CountRemainWalls();
        SetScore(true, stageCount, remainWalls);
        yield return new WaitForSeconds(countDuration);

        //FinalStage
        stageCount++;
        stageDuration = 30f;
        //stageDuration = 10f;

        maxBallSpeed = 9f;
        gameAudio.Play(gameAudio.finalStage);
        StartStage(stageDuration);
        SetWalls();
        this.CallMethodDelayed(stageDuration - 10f, () => audioController.OnCountTen());
        this.Invoke(CountDown, stageDuration - 5f);
        yield return new WaitForSeconds(stageDuration);

        ballController.ClearBalls();
        timer.Hide();
        remainWalls = CountRemainWalls();
        SetScore(true, stageCount, remainWalls);
        //SetScore will call osc send if stageCount == 2

        yield return new WaitForSeconds(1f);
        GetResult();
    }


    IEnumerator ExplainRoutine()
    {
        yield return new WaitForSeconds(2f);
        var t = 0f;
        var r = teamObj.GetComponent<Renderer>();
        var posFrom = teamObj.transform.position;
        var posTo = posFrom + Vector3.down * 2f;
        while (t < 1f)
        {
            teamObj.transform.position = Vector3.Lerp(posFrom, posTo, t);
            r.SetColor("_Color", ProjectionController.MyColor * (1f + t * t * 5f));
            yield return t += Time.deltaTime / 5f;
        }
        t = 0f;
        while (t < 1f)
        {
            kinectVis.SetFloat("_Amount", t);
            teamObj.transform.localScale = Vector3.one * (1f - t);
            foreach (var fr in floorRenderers)
                fr.SetFloat("_T", t * 0.25f);
            yield return t += Time.deltaTime / 2f
;
        }
        Destroy(teamObj);
        BallController.Instance.AddBall(centerPos.position);
        yield return new WaitForSeconds(1f);
        SetWalls();
    }

    void StartStage(float stageDuration)
    {
        ballController.AddBallWithTimeLine(stageCount);
        StartCoroutine(StartStageRoutine());
        timer.StartStage(stageDuration);
    }
    IEnumerator StartStageRoutine()
    {
        stageTexts.ShowTitle(stageCount);
        yield return StartCoroutine(fader.FadeRoutine(0.5f));
        yield return new WaitForSeconds(1f);
        gameAudio.Play(gameAudio.start);
        stageTexts.ShowStart();
        yield return StartCoroutine(fader.FadeRoutine(0.0f));
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

    int CountRemainWalls()
    {
        var count = currentWallList.Count(w => !w.broken);
        StartCoroutine(WallCountAction());
        return count;
    }
    IEnumerator WallCountAction()
    {
        yield return new WaitForSeconds(1f);
        wallCountSound.Play();
        var allWalls = currentWallList.Count;
        var wallRemains = currentWallList.Where(w => !w.broken);
        var colors = wallRemains.Select(w => w.color);
        var t = 0f;
        while (t < 1f)
        {
            for (var i = 0; i < wallRemains.Count(); i++)
            {
                var w = wallRemains.ElementAt(i);
                var col = w.color;
                col = Color.Lerp(col, ProjectionController.MyColor * 2f, t);
                w.renderer.SetColor("_Color", col);
            }
            yield return t += Time.deltaTime;
        }

        foreach (var w in wallRemains)
        {
            w.endGame = true;
            StartCoroutine(WallToFloorRoutine(w));
            yield return new WaitForSeconds(0.05f);
        }
        wallCountSound.Stop();
    }
    IEnumerator WallToFloorRoutine(Wall wall)
    {
        keptWallList.Add(wall);
        var idx = keptWallList.Count - 1;
        var x = idx % 10;
        var y = idx / 10;

        var pos0 = wall.transform.position;
        var pos1 = centerPos.position;
        pos1.y = 0f;
        var pos2 = pos1 + Vector3.down * 20f + Vector3.right * (x - 5f) + Vector3.back * (y - 5f);
        var vel0 = Vector3.up * 10f;
        var vel1 = Vector3.down * 5f;
        var vel2 = Vector3.zero;
        var chain = new CoonsChain();
        chain.positions = new[] { pos0, pos1, pos2 };
        chain.velocities = new[] { vel0, vel1, vel2 };
        chain.UpdateCurves();
        var t = 0f;
        while (t < 1f)
        {
            wall.transform.position = chain.Interpolate(t);
            yield return t += Time.deltaTime / 2f;
        }
        foreach (var r in floorRenderers)
            r.SetFloat("_T", 0.25f + idx * 0.01f);
        t = 0f;
        while (t < 1f)
        {
            var col = Color.Lerp(ProjectionController.MyColor, ProjectionController.MyColor * 0.5f, t);
            wall.renderer.SetColor("_Color", col);
            yield return t + Time.deltaTime;
        }
    }

    void CountDown()
    {
        StartCoroutine(FinishRoutine());
        gameAudio.Play(gameAudio.countFive);
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

    private void Update()
    {
        if (end)
            return;
        for (var i = 0; i < keptWallList.Count; i++)
        {
            var w = keptWallList[i];
            var axis = Quaternion.AngleAxis(4f * i, Vector3.up) * Vector3.forward;
            w.transform.Rotate(axis, Time.deltaTime * 45f);
        }
    }

    [System.Serializable]
    public class Setting : SettingManager.Setting
    {
        public bool isLeft;
    }
    [System.Serializable]
    public class Fader
    {
        public Renderer fadeRenderer;
        public float fadeVal;
        public void Fade(float f)
        {
            var col = Color.black;
            col.a = f * 0.5f;
            fadeVal = f;
            fadeRenderer.SetColor("_TintColor", col);
        }
        public IEnumerator FadeRoutine(float fadeTo, float duration = 1f)
        {
            var fadeFrom = fadeVal;
            var t = 0f;
            while(t < 1f)
            {
                var f = Mathf.Lerp(fadeFrom, fadeTo, t);
                Fade(f);
                yield return t += Time.deltaTime / duration;
            }
            Fade(fadeTo);
        }
    }

    [System.Serializable]
    public struct GameAudio
    {
        public AudioClip titleStartSe;
        public AudioClip firstStage;
        public AudioClip secondStage;
        public AudioClip finalStage;
        public AudioClip start;
        public AudioClip countFive;
        public AudioClip finish;
        public AudioClip wallKeeper;
        public AudioClip winnerIs;
        public AudioClip team_blue;
        public AudioClip team_red;
        public AudioClip endingSe;

        public AudioSource audioPlayer;
        public void Play(AudioClip clip)
        {
            if (clip == null)
                return;
            audioPlayer.clip = clip;
            audioPlayer.Play();
        }
    }
}
