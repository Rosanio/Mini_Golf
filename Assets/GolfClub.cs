using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfClub : MonoBehaviour
{
    private float yPosition;

    GameObject golfBall;

    void Start()
    {
        yPosition = transform.position.y;
        golfBall = GameObject.Find("GolfBall");
    }

    void Update()
    {
        Plane plane = new Plane(Vector3.up, -yPosition);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 mouseInWorld = ray.GetPoint(distance);
            Vector3 golfBallPosition = golfBall.transform.position;
            Vector3 clubToBallVector = mouseInWorld - golfBallPosition;
            clubToBallVector.Normalize();

            transform.position = golfBallPosition + clubToBallVector;
        }
    }
}