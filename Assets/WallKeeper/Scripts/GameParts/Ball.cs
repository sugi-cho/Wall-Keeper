using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using sugi.cc;
using Osc;

public class Ball : RendererBehaviour
{
    GameController controller { get { return GameController.Instance; } }
    BallController ballController { get { return BallController.Instance; } }

    public bool ignoreTrigger;
    public float minSpeed = 1f;
    public float maxSpeed { get { return controller.maxBallSpeed; } }

    public new Rigidbody rigidbody { get { if (_rigid == null) _rigid = GetComponent<Rigidbody>(); return _rigid; } }
    Rigidbody _rigid;
    public new Collider collider { get { if (_coll == null) _coll = GetComponent<Collider>(); return _coll; } }
    Collider _coll;

    int hitCount;
    float speed;

    public void SetHitCount(int count)
    {
        hitCount = count;
    }

    private void FixedUpdate()
    {
        speed = Mathf.Lerp(minSpeed, maxSpeed, hitCount / 100f);
        rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, speed);
        var pos = rigidbody.position;
        pos.y = ballController.transform.position.y;
        rigidbody.position = pos;
    }

    private void OnCollisionEnter(Collision collision)
    {
        var wall = collision.gameObject.GetComponent<Wall>();
        var atackPoint = collision.gameObject.GetComponent<AtackPoint>();
        if (wall == null && atackPoint == null)
            AudioController.Instance.OnBodyHit();
        var reflection = atackPoint == null ? 1f / 5f : 1f / 3f;
        rigidbody.AddForce(collision.contacts[0].normal * speed * reflection, ForceMode.Impulse);
        rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, speed);
        hitCount++;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (ignoreTrigger) return;
        var transfer = other.gameObject.GetComponent<TransferObj>();
        if (transfer != null)
        {
            var osc = new MessageEncoder("/transfer");
            osc.Add(-rigidbody.position.x);
            osc.Add(rigidbody.position.y);
            osc.Add(rigidbody.position.z);
            osc.Add(rigidbody.velocity.x);
            osc.Add(rigidbody.velocity.y);
            osc.Add(rigidbody.velocity.z);
            osc.Add(hitCount);
            OscController.Instance.Send(osc);

            var vel = rigidbody.velocity;
            vel.x *= -1f;
            rigidbody.velocity = vel;

            AudioController.Instance.OnPassThrogh();
        }
    }

    private void Start()
    {
        this.CallMethodDelayed(0.2f, () => ignoreTrigger = false);
    }
}
