using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfBall : MonoBehaviour
{
    public bool IsMoving = false;

    private Rigidbody rigidBody;
    private Vector3 lastVelocity;
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        lastVelocity = rigidBody.velocity;

        if (IsMoving && lastVelocity.magnitude < 0.5f)
        {
            rigidBody.velocity = new Vector3(0, 0, 0);
            rigidBody.angularVelocity = new Vector3(0, 0, 0);
            IsMoving = false;
        }
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            float speed = lastVelocity.magnitude;
            Vector3 direction = Vector3.Reflect(lastVelocity.normalized, collision.contacts[0].normal);
            rigidBody.velocity = direction * speed;
        }
    }
}
