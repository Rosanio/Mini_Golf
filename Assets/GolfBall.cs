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
    private Collision wallCollision;
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
        print("Collision enter");
        if (collision.gameObject.tag == "Wall")
        {
            if (lastVelocity != Vector3.zero)
            {
                rigidBody.velocity = GetVelocityOnCollision(lastVelocity, collision);
            }
            else
            {
                rigidBody.velocity = GetVelocityOnCollision(lastSwingForce, collision);
            }
        }
    }

    protected void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            wallCollision = collision;
        }
    }

    protected void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            wallCollision = null;
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
        if (wallCollision != null)
        {
            // If the ball landed in a spot where it's already colliding with a wall, and the player swings into the wall,
            // the ball should reflect in the opposite direction
            Vector3 wallNormal = wallCollision.contacts[0].normal;
            float angle = Vector3.Angle(swingVector, wallNormal);
            if (angle > 90)
            {
                rigidBody.velocity = GetVelocityOnCollision(swingVector, wallCollision);
            }
        }
        else
        {
            rigidBody.AddForce(swingVector, ForceMode.Impulse);
        }
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
