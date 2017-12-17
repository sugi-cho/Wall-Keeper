using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using sugi.cc;

public class Wall : RendererBehaviour
{

    public int hitCount = 0;
    public Side side;
    public Color color { get; private set; }
    public bool broken { get; private set; }
    new Collider collider { get { if (_c == null) _c = GetComponent<Collider>(); return _c; } }
    Collider _c;

    GameController controller { get { return GameController.Instance; } }
    public bool endGame;

    public void Hide()
    {
        renderer.enabled = false;
        collider.enabled = false;
    }
    public void Show()
    {
        color = ProjectionController.MyColor;
        renderer.enabled = true;
        collider.enabled = true;
        hitCount = 0;
        broken = false;
        StartCoroutine(ShowRoutine());
    }
    IEnumerator ShowRoutine()
    {
        var toPos = transform.position;
        var fromPos = toPos + Vector3.down * 5f * Random.value;
        var rndDuration = Random.Range(0.5f, 1f);
        var t = 0f;
        renderer.SetColor("_Color", color * 2f);
        while (t < 1f)
        {
            transform.position = Vector3.Lerp(fromPos, toPos, t);
            transform.localScale = t * Vector3.one;
            yield return t += Time.deltaTime / rndDuration;
        }
        transform.position = toPos;
        transform.localScale = Vector3.one;
        yield return new WaitForSeconds(0.5f);
        renderer.SetColor("_Color", color);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (endGame)
            return;
        var ball = collision.gameObject.GetComponent<Ball>();
        if (ball != null)
        {
            hitCount++;
            if (controller.breakCount <= hitCount)
                BreakWall();
            else
                AddDamage(hitCount);
        }
    }

    void BreakWall()
    {
        AudioController.Instance.OnWallBreak();
        broken = true;
        collider.enabled = false;
        renderer.enabled = false;
    }

    void AddDamage(int count)
    {
        AudioController.Instance.OnWallHit();
        renderer.SetColor("_Color", ProjectionController.MyColor * 0.25f);
        color = ProjectionController.MyColor * 0.25f;
    }

    private void Start()
    {
        Hide();
    }

#if UNITY_EDITOR
    [MenuItem("Custom/Select/WallsInChildren")]
    public static void Select()
    {
        var go = Selection.activeGameObject;
        if (go != null)
            Selection.objects = go.GetComponentsInChildren<Wall>().Select(w => w.gameObject).ToArray();
    }
    [MenuItem("Custom/Select/RenderersInC")]
    public static void SelectR()
    {
        var go = Selection.activeGameObject;
        if (go != null)
            Selection.objects = go.GetComponentsInChildren<Renderer>().Select(r => r.gameObject).ToArray();
    }
    [MenuItem("Custom/AddComponents/Wall")]
    public static void AddWallComponents()
    {
        var go = Selection.activeGameObject;
        var rs = go.GetComponentsInChildren<Renderer>();
        foreach (var r in rs)
        {
            var wall = r.GetComponent<Wall>();
            if (wall == null)
                r.gameObject.AddComponent<Wall>();
        }
    }
#endif

    public enum Side
    {
        Left,
        Right,
    }
}
