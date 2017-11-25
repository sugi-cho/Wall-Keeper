using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public float minSpeed = 1f;
    public float maxSpeed = 5f;
    public new Rigidbody rigidbody;
    public bool ignoreTrigger;

    int hitCount;
    float speed;

    public void SetHitCount(int count)
    {
        hitCount = count;
    }

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        speed = Mathf.Lerp(minSpeed, maxSpeed, hitCount / 100f);
        rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, speed);
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
}
