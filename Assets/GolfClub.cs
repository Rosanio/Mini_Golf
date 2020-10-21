using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GolfClub : MonoBehaviour
{
    private GameObject golfBall;
    private float yOffsetFromGround;

    void Start()
    {
        golfBall = GameObject.Find("GolfBall");
        yOffsetFromGround = transform.position.y - golfBall.transform.position.y;
    }

    void Update()
    {
        Vector3 golfBallPosition = golfBall.transform.position;
        Plane plane = new Plane(Vector3.up, -golfBallPosition.y);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 mouseInWorld = ray.GetPoint(distance);
            Vector3 clubToBallVector = mouseInWorld - golfBallPosition;
            clubToBallVector.Normalize();

            transform.position = golfBallPosition + clubToBallVector + new Vector3(0, yOffsetFromGround, 0);
        }
    }
}