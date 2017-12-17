using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using sugi.cc;

public class BallController : SingletonMonoBehaviour<BallController>
{

    public Ball ballPrefab;
    public List<Ball> ballList;
    public AddBallTimeLine[] addBallTimelines;
    public float ballScale = 1f;

    public void TransferBall(Vector3 pos, Vector3 vel, int hitCount)
    {
        var newBall = Instantiate(ballPrefab);
        newBall.transform.localScale = ballScale * Vector3.one;
        newBall.rigidbody.position = pos;
        newBall.rigidbody.velocity = vel;
        newBall.SetHitCount(hitCount);
        newBall.ignoreTrigger = true;
        ballList.Add(newBall);
    }
    public void AddBallWithTimeLine(int idx)
    {
        StartCoroutine(addBallTimelines[idx].AddBallRoutine());
    }
    public void AddBall(Vector3 pos, float duration = 1f)
    {
        var newBall = Instantiate(ballPrefab);
        newBall.transform.position = pos;
        newBall.transform.SetParent(transform);
        StartCoroutine(BallInRroutine(newBall, duration));
        ballList.Add(newBall);
    }
    IEnumerator BallInRroutine(Ball ball, float duration)
    {
        var pos = ball.transform.localPosition;
        pos.y = 0f;
        ball.transform.localPosition = pos;
        var t = 0f;
        while (t < 1f)
        {
            ball.transform.localScale = t * ballScale * Vector3.one;
            yield return t += Time.deltaTime / duration;
        }
        yield return new WaitForSeconds(2.0f);
        var vel = Random.onUnitSphere;
        vel.y = 0;
        vel = vel.normalized * GameController.Instance.maxBallSpeed * 0.5f;
        ball.rigidbody.velocity = vel;
    }
    public void RemoveBall(Ball b)
    {
        ballList.Remove(b);
        Destroy(b.gameObject);
    }
    public void ClearBalls()
    {
        for (var i = 0; i < ballList.Count; i++)
            Destroy(ballList[i].gameObject);
        ballList.Clear();
    }

    [System.Serializable]
    public struct AddBallTime
    {
        public float time;
        public Vector3 deltaFromCenter;
        public bool isRand;

        public void AddBall()
        {
            var delta = deltaFromCenter;
            if (isRand)
                delta = Random.insideUnitSphere * 1.5f;
            var pos = TextPositionSetter.CenterPos + delta;
            Instance.AddBall(pos);
        }
    }
    [System.Serializable]
    public struct AddBallTimeLine
    {
        public AddBallTime[] timeline;
        public IEnumerator AddBallRoutine()
        {
            var sortedTL = timeline.OrderBy(t => t.time);
            var time = 0f;
            foreach (var t in sortedTL)
            {
                yield return new WaitForSeconds(t.time - time);
                t.AddBall();
                time = t.time;
            }
        }
    }
}
