using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Wall : MonoBehaviour {

    public int hitCount = 0;
    public int breakCount = 3;
    public Side side;

    private void OnCollisionEnter(Collision collision)
    {
        var ball = collision.gameObject.GetComponent<Ball>();
        if(ball!=null)
        {
            hitCount++;
            if (hitCount == breakCount)
                BreakWall();
            else
                AddDamage(hitCount);
        }
    }

    void BreakWall()
    {
        AudioController.Instance.OnWallBreak();
        Destroy(gameObject);
    }

    void AddDamage(int count)
    {
        AudioController.Instance.OnWallHit();
    }

#if UNITY_EDITOR
    [MenuItem("Custom/Select/WallsInChildren")]
    public static void Select()
    {
        var go = Selection.activeGameObject;
        if (go != null)
            Selection.objects = go.GetComponentsInChildren<Wall>();
    }
#endif

    public enum Side
    {
        Left,
        Right,
    }
}
