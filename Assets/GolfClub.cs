using System;
using System.Net.WebSockets;
using UnityEngine;

public class GolfClub : MonoBehaviour
{
    [SerializeField] float forceMultiplier = 5.0f;

    // Game State
    private enum State { Positioning, Swinging, BallMoving }
    private State currentState;

    // Useful reference values and vectors
    private float yOffsetClubToBall;
    private Vector3 clubToBallVector;
    private float swingDistance;

    // Cached transform values
    private Quaternion positionRotation;
    private Vector3 swingPosition;
    private Quaternion swingRotation;
    private Vector3 perpendicularToSwing;
    private Plane swingThresholdPlane;
    private Vector3 golfBallPosition;

    private GameObject golfBall;
    private Rigidbody golfBallRigidBody;

    private const float Z_OFFSET_CLUB_TO_BALL = 0.1f;
    private const float MAX_SWING_DISTANCE = 2f;

    void Start()
    {
        golfBall = GameObject.Find("GolfBall");
        golfBallRigidBody = golfBall.GetComponent<Rigidbody>();
        yOffsetClubToBall = transform.position.y - golfBall.transform.position.y;
        currentState = State.Positioning;

        positionRotation = transform.rotation;

        golfBallPosition = golfBall.transform.position;
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Positioning:
                Positioning();
                break;
            case State.Swinging:
                Swinging();
                break;
            case State.BallMoving:
                BallMoving();
                break;
        }
    }

    private void Positioning()
    {
        SetGolfClubPosition();

        // Left mouse button click
        if (Input.GetMouseButtonDown(0))
        {
            InitializeSwingingState();
        }
    }

    private void Swinging()
    {
        SetGolfClubSwing();

        // Left mouse button click
        if (Input.GetMouseButtonDown(0))
        {
            Swing();
            // Set state after delay to allow ball to pick up speed, preventing race condition where state is
            // immediately reset to positioning.
            Invoke("SetStateBallMoving", 0.5f);
        }

        // Right mouse button click
        if (Input.GetMouseButtonDown(1))
        {
            currentState = State.Positioning;
        }
    }

    private void BallMoving()
    {
        CheckBallMovementComplete();
    }

    private void SetGolfClubPosition()
    {
        Vector3 mouseInWorld = GetMousePositionInWorld();
        clubToBallVector = mouseInWorld - golfBallPosition;
        clubToBallVector.Normalize();

        float angle = Mathf.Atan2(clubToBallVector.z, clubToBallVector.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.down);

        transform.position = golfBallPosition + clubToBallVector*0.5f + new Vector3(0, yOffsetClubToBall, Z_OFFSET_CLUB_TO_BALL);
        transform.rotation = q * positionRotation;
    }

    private void SetGolfClubSwing()
    {
        Vector3 mouseInWorld = GetMousePositionInWorld();
        Vector3 mouseToBallVector = mouseInWorld - golfBallPosition;

        Vector3 projectedMouseToBallVector = Vector3.Project(mouseToBallVector, clubToBallVector);
        swingDistance = GetConstrainedSwingDistance(projectedMouseToBallVector);

        Vector3 clubRotationPivotPoint = swingPosition + new Vector3(0, 1.5f, 0);
        float swingAngle = swingDistance * 30.0f;
        Quaternion clubRotation = Quaternion.AngleAxis(swingAngle, perpendicularToSwing);

        transform.position = clubRotationPivotPoint - (clubRotation * (clubRotationPivotPoint - swingPosition));
        transform.rotation = clubRotation * swingRotation;
    }

    private void InitializeSwingingState()
    {
        swingPosition = golfBallPosition + clubToBallVector * 0.1f + new Vector3(0, yOffsetClubToBall, Z_OFFSET_CLUB_TO_BALL); ;
        swingRotation = transform.rotation;
        perpendicularToSwing = Vector3.Cross(clubToBallVector, Vector3.up);
        swingThresholdPlane = new Plane(clubToBallVector, Vector3.up);
        currentState = State.Swinging;
    }

    private void Swing()
    {
        golfBall.GetComponent<Rigidbody>().AddForce(clubToBallVector.normalized * -(forceMultiplier * swingDistance), ForceMode.Impulse);
    }

    // Defined as a separate function so Invoke can be called correctly
    private void SetStateBallMoving()
    {
        currentState = State.BallMoving;
    }

    private void CheckBallMovementComplete()
    {
        if (golfBallRigidBody.velocity.magnitude < 0.5f)
        {
            golfBallRigidBody.velocity = new Vector3(0, 0, 0);
            golfBallRigidBody.angularVelocity = new Vector3(0, 0, 0);
            golfBallPosition = golfBall.transform.position;
            currentState = State.Positioning;
        }
    }

    private Vector3 GetMousePositionInWorld()
    {
        Plane plane = new Plane(Vector3.up, -golfBall.transform.position.y);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // Since the camera is facing the plane we're using, it should be impossible for this function to fail.
        plane.Raycast(ray, out float distance);
        return ray.GetPoint(distance);
    }

    private float GetConstrainedSwingDistance(Vector3 projectedMouseToBallVector)
    {
        float swingDistance = Vector3.Distance(projectedMouseToBallVector, new Vector3(0, 0, 0));
        if (swingDistance > MAX_SWING_DISTANCE)
        {
            swingDistance = MAX_SWING_DISTANCE;
        }

        // If the mouse is not on the same side of the ball as the club, assume the minimum possible distance
        if (!swingThresholdPlane.GetSide(projectedMouseToBallVector))
        {
            swingDistance = 0;
        }
        return swingDistance;
    }
}