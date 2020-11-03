using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfBall : MonoBehaviour
{
    [HideInInspector] public bool IsMoving = false;
    [HideInInspector] public bool ballInHole = false;

    [SerializeField] ParticleSystem SuccessParticles = null;

    private Rigidbody rigidBody;
    private Vector3 lastVelocity;
    private Vector3 lastSwingForce;
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        lastVelocity = rigidBody.velocity;

        
        if (IsMoving && lastVelocity.magnitude < 0.5f)
        {
            StopMoving();
            lastVelocity = Vector3.zero;
        }
        
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            if (lastVelocity != Vector3.zero)
            {
                print("Using last velocity");
                print(lastVelocity.magnitude);
                rigidBody.velocity = GetVelocityOnCollision(lastVelocity, collision);
            }
            else
            {
                print("Using current velocity");
                print(lastSwingForce.magnitude);
                rigidBody.velocity = GetVelocityOnCollision(lastSwingForce, collision);
            }
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Finish")
        {
            StopMoving();
            SuccessParticles.Play();
            ballInHole = true;
        }
    }

    public void Hit(Vector3 swingVector)
    {
        rigidBody.AddForce(swingVector, ForceMode.Impulse);
        lastSwingForce = swingVector;
    }

    private void StopMoving()
    {
        rigidBody.velocity = new Vector3(0, 0, 0);
        rigidBody.angularVelocity = new Vector3(0, 0, 0);
        IsMoving = false;
    }

    private Vector3 GetVelocityOnCollision(Vector3 lastVelocity, Collision collision)
    {
        float speed = lastVelocity.magnitude;
        Vector3 direction = Vector3.Reflect(lastVelocity.normalized, collision.contacts[0].normal);
        return direction * speed;
    }
}
