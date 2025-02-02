using System;
using UnityEngine;

public class DirectionTester : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int iLever = 0; iLever < 6; iLever++) {
            for (int quarterTurns = 0; quarterTurns < 4; quarterTurns++) {
                for (int iAxis = 0; iAxis < 6; iAxis++) {
                    var lever = (Direction) iLever;
                    var axis = (Direction) iAxis;
                    var funky = lever.Rotate(quarterTurns, axis);
                    var reliable = (
                        Quaternion.AngleAxis(90.0f*quarterTurns, axis.ToVector3()) *
                        lever.ToVector3()
                    ).ToDirection();
                    Debug.Log(
                        $"rotate {lever} {quarterTurns} qts about {axis} => {funky}; " +
                        $"reliable method says: {reliable}"
                    );
                }
            }
        }
    }
}
