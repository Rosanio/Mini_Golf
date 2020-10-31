using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class GolfClub : MonoBehaviour
{
    [SerializeField] float forceMultiplier = 5.0f;
    [SerializeField] SpriteRenderer arrow = null;
    [SerializeField] Slider swingPowerSlider = null;
    [SerializeField] float swingAnimationSpeed = 1.0f;

    // Game State
    private enum State { Positioning, Swinging, PlayingSwingAnimation, BallMoving, LevelComplete }
    private State currentState;

    // Useful reference values and vectors
    private float yOffsetClubToBall;
    private Vector3 clubToBallVector;
    private float swingDistance;
    private float swingAnimationDegreesPerSecond;

    // Cached transform values
    private Quaternion positionRotation;
    private Vector3 swingPosition;
    private Quaternion swingRotation;
    private Vector3 perpendicularToSwing;
    private Plane swingThresholdPlane;
    private Vector3 golfBallPosition;
    private Vector3 swingPivotPoint;

    // Golf Ball References
    private GameObject golfBallGameObject;
    private GolfBall golfBall;

    // Constants
    private const float Z_OFFSET_CLUB_TO_BALL = -0.1f;
    private const float MAX_SWING_DISTANCE = 2f;

    void Start()
    {
        // Initialize instance variables
        golfBallGameObject = GameObject.Find("GolfBall");
        golfBall = golfBallGameObject.GetComponent<GolfBall>();
        yOffsetClubToBall = transform.position.y - golfBallGameObject.transform.position.y;
        
        // Initialize positioning state
        currentState = State.Positioning;
        positionRotation = transform.rotation;
        golfBallPosition = golfBallGameObject.transform.position;

        // Initialize UI
        arrow.enabled = true;
        swingPowerSlider.value = 0;
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
            case State.PlayingSwingAnimation:
                PlayingSwingAnimation();
                break;
            case State.BallMoving:
                BallMoving();
                break;
            default:
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
            PlaySwingAnimation();
        }

        // Right mouse button click
        if (Input.GetMouseButtonDown(1))
        {
            currentState = State.Positioning;
        }
    }

    private void PlayingSwingAnimation()
    {
        float zAngle = transform.rotation.eulerAngles.z - (swingAnimationDegreesPerSecond * Time.deltaTime);
        print(zAngle);
        // zAngle resets to 360 when it goes to negatives, this will stop it rotating at -20 degrees
        if (zAngle < 300.0f || zAngle > 330.0f)
        {
            RotateClubAboutPoint(zAngle, swingPivotPoint);
        }
        if (zAngle < Mathf.Epsilon)
        {
            Swing();
            // Set state after delay to allow ball to pick up speed, preventing race condition where state is
            // immediately reset to positioning.
            Invoke(nameof(SetStateBallMoving), 0.5f);
        }
    }

    private void BallMoving()
    {
        CheckBallMovementComplete();
    }

    private void SetGolfClubPosition()
    {
        Vector3 mouseInWorld = GetMousePositionInWorld();
        golfBallPosition = golfBallGameObject.transform.position;
        clubToBallVector = mouseInWorld - golfBallPosition;
        clubToBallVector.Normalize();

        float angle = Mathf.Atan2(clubToBallVector.z, clubToBallVector.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.down);

        transform.position = golfBallPosition + clubToBallVector*0.5f + new Vector3(0, yOffsetClubToBall, Z_OFFSET_CLUB_TO_BALL);
        transform.rotation = q * positionRotation;

        arrow.transform.position = golfBallPosition - clubToBallVector;
        arrow.transform.rotation = q * Quaternion.AngleAxis(90f, Vector3.right) * Quaternion.AngleAxis(180f, Vector3.up);
    }

    private void SetGolfClubSwing()
    {
        Vector3 mouseInWorld = GetMousePositionInWorld();
        Vector3 mouseToBallVector = mouseInWorld - golfBallPosition;

        Vector3 projectedMouseToBallVector = Vector3.Project(mouseToBallVector, clubToBallVector);
        swingDistance = GetConstrainedSwingDistance(projectedMouseToBallVector);

        swingPivotPoint = swingPosition + new Vector3(0, 1.5f, 0);
        float swingAngle = swingDistance * 30.0f;
        RotateClubAboutPoint(swingAngle, swingPivotPoint);

        swingPowerSlider.value = swingDistance / MAX_SWING_DISTANCE;
    }

    private void InitializeSwingingState()
    {
        swingPosition = golfBallPosition + clubToBallVector * 0.1f + new Vector3(0, yOffsetClubToBall, Z_OFFSET_CLUB_TO_BALL); ;
        swingRotation = transform.rotation;
        perpendicularToSwing = Vector3.Cross(clubToBallVector, Vector3.up);
        swingThresholdPlane = new Plane(clubToBallVector, Vector3.up);
        currentState = State.Swinging;
    }

    private void PlaySwingAnimation()
    {
        swingAnimationDegreesPerSecond = transform.rotation.eulerAngles.z / swingAnimationSpeed;
        currentState = State.PlayingSwingAnimation;
    }

    private void Swing()
    {
        golfBallGameObject.GetComponent<Rigidbody>().AddForce(clubToBallVector.normalized * -(forceMultiplier * swingDistance), ForceMode.Impulse);
    }

    // Defined as a separate function so Invoke can be called correctly
    private void SetStateBallMoving()
    {
        golfBall.IsMoving = true;
        currentState = State.BallMoving;
    }

    private void CheckBallMovementComplete()
    {
        if (!golfBall.IsMoving)
        {
            if (golfBall.ballInHole)
            {
                currentState = State.LevelComplete;
                print("Level complete!");
            }
            else
            {
                golfBallPosition = golfBallGameObject.transform.position;
                swingPowerSlider.value = 0;
                currentState = State.Positioning;
            }
        }
    }

    private Vector3 GetMousePositionInWorld()
    {
        Plane plane = new Plane(Vector3.up, -golfBallGameObject.transform.position.y);
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

    private void RotateClubAboutPoint(float angle, Vector3 pivotPoint)
    {
        Quaternion clubRotation = Quaternion.AngleAxis(angle, perpendicularToSwing);

        transform.position = pivotPoint - (clubRotation * (pivotPoint - swingPosition));
        transform.rotation = clubRotation * swingRotation;
    }
}