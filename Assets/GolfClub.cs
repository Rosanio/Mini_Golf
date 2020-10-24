using System;
using UnityEngine;

public class GolfClub : MonoBehaviour
{
    // Game State
    private enum State { Positioning, Swinging, }
    private State currentState;

    // Useful reference values and vectors
    private float yOffsetClubToBall;
    private Vector3 clubToBallVector;

    // Cached transform values
    private Quaternion positionRotation;
    private Vector3 swingPosition;
    private Quaternion swingRotation;
    private Vector3 golfBallPosition;

    private GameObject golfBall;

    private const float zOffsetClubToBall = 0.1f;

    void Start()
    {
        golfBall = GameObject.Find("GolfBall");
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
        }
    }

    private void Positioning()
    {
        SetGolfClubPosition();

        // Left mouse button click
        if (Input.GetMouseButtonDown(0))
        {
            InitializeSwingState();
        }
    }

    private void Swinging()
    {
        SetGolfClubSwing();

        // Right mouse button click
        if (Input.GetMouseButtonDown(1))
        {
            currentState = State.Positioning;
        }
    }

    private void SetGolfClubPosition()
    {
        Vector3 mouseInWorld = GetMousePositionInWorld();
        clubToBallVector = mouseInWorld - golfBallPosition;
        clubToBallVector.Normalize();

        float angle = Mathf.Atan2(clubToBallVector.z, clubToBallVector.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.down);

        transform.position = golfBallPosition + clubToBallVector*0.5f + new Vector3(0, yOffsetClubToBall, zOffsetClubToBall);
        transform.rotation = q * positionRotation;
    }

    private void SetGolfClubSwing()
    {
        Vector3 mouseInWorld = GetMousePositionInWorld();
        Vector3 mouseToBallVector = mouseInWorld - golfBallPosition;

        Vector3 projectedMouseToBallVector = Vector3.Project(mouseToBallVector, clubToBallVector);
        float projectedDistance = Vector3.Distance(projectedMouseToBallVector, new Vector3(0, 0, 0));

        Vector3 clubRotationPivotPoint = swingPosition + new Vector3(0, 1.5f, 0);
        float angle = projectedDistance * 30.0f;
        Vector3 perpendicularToBall = Vector3.Cross(projectedMouseToBallVector, Vector3.up);
        Quaternion clubRotation = Quaternion.AngleAxis(angle, perpendicularToBall);

        transform.position = clubRotationPivotPoint - (clubRotation * (clubRotationPivotPoint - swingPosition));
        transform.rotation = clubRotation * swingRotation;
    }

    private Vector3 GetMousePositionInWorld()
    {
        Plane plane = new Plane(Vector3.up, -golfBall.transform.position.y);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // Since the camera is facing the plane we're using, it should be impossible for this function to fail.
        plane.Raycast(ray, out float distance);
        return ray.GetPoint(distance);
    }

    private void InitializeSwingState()
    {
        swingPosition = golfBallPosition + clubToBallVector * 0.1f + new Vector3(0, yOffsetClubToBall, zOffsetClubToBall); ;
        swingRotation = transform.rotation;
        currentState = State.Swinging;
    }
}