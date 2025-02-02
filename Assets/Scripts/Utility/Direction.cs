using System;
using UnityEngine;

[System.Serializable]
public enum Direction {
    PositiveX,
    NegativeX,
    PositiveY,
    NegativeY,
    PositiveZ,
    NegativeZ,
    Up = PositiveY,
    Down = NegativeY,
    Left = NegativeX,
    Right = PositiveX,
    Forward = PositiveZ,
    Back = NegativeZ
}
