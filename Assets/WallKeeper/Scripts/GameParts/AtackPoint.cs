using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Windows.Kinect;
using sugi.cc;

public class AtackPoint : RendererBehaviour
{
    GameController controller { get { return GameController.Instance; } }

    bool isTracked;
    public ulong trackingId;
    public Vector3 position
    {
        set
        {
            if (transform == null)
                transform = GetComponent<Transform>();
            transform.position = value;
        }
    }
    new Transform transform;
    new Rigidbody rigidbody;
    new Collider collider;
    
    public void SetPos(Vector3 pos)
    {
        SetTracked();
        var ray = new Ray(controller.gameCam.transform.position, pos - controller.gameCam.transform.position);
        float enter;
        if (controller.gamePlane.Raycast(ray, out enter))
            rigidbody.position = ray.GetPoint(enter);
    }

    public void SetUntracked()
    {
        isTracked = false;
        collider.enabled = false;
        renderer.enabled = false;
    }
    void SetTracked()
    {
        isTracked = true;
        collider.enabled = true;
        renderer.enabled = true;
    }

    private void Start()
    {
        transform = GetComponent<Transform>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        SetUntracked();
    }

    private void OnCollisionEnter(Collision collision)
    {
        var ball = collision.gameObject.GetComponent<Ball>();
        if (ball != null)
        {
            AudioController.Instance.OnBodyHit();
        }
    }
}
