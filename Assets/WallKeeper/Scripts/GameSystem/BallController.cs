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

    public void TransferBall(Vector3 pos, Vector3 vel,int hitCount, bool isLeft)
    {
        var newBall = Instantiate(ballPrefab);
        newBall.rigidbody.position = pos;
        newBall.rigidbody.velocity = vel;
        newBall.SetHitCount(hitCount);
        newBall.isLeft = isLeft;
        newBall.ignoreTrigger = true;
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
        newBall.isLeft = GameController.Instance.setting.isLeft;
        StartCoroutine(BallInRroutine(newBall, duration));
        ballList.Add(newBall);
    }
    IEnumerator BallInRroutine(Ball ball, float duration)
    {
        ball.collider.enabled = false;
        ball.rigidbody.isKinematic = true;
        var pos = ball.transform.localPosition;
        var fromY = duration * 10f;
        var t = 0f;
        while (t < 1f)
        {
            pos.y = Mathf.Lerp(fromY, 0, t);
            ball.transform.localPosition = pos;
            yield return t += Time.deltaTime / duration;
        }
        pos.y = 0f;
        ball.collider.enabled = true;
        ball.rigidbody.isKinematic = false;
        yield return new WaitForSeconds(1f);
        ball.rigidbody.velocity = Random.onUnitSphere;
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
