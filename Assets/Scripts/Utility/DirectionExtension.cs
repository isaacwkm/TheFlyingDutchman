using System;
using UnityEngine;

public static class DirectionExtension {
    public static Vector3 ToVector3(this Direction self) {
        return (Vector3) self.ToVector3Int();
    }
    public static Direction ToDirection(this Vector3 self) {
        Direction result = (Direction) 0;
        float confidence = -1.0f;
        self = self.normalized;
        foreach (Direction candidate in Enum.GetValues(typeof(Direction))) {
            float appropriacy = Vector3.Dot(self, candidate.ToVector3());
            if (appropriacy >= confidence) {
                result = candidate;
                confidence = appropriacy;
            }
        }
        return result;
    }
    public static Vector3Int ToVector3Int(this Direction self) {
        switch (self) {
            case Direction.PositiveX: return Vector3Int.right;
            case Direction.NegativeX: return Vector3Int.left;
            case Direction.PositiveY: return Vector3Int.up;
            case Direction.NegativeY: return Vector3Int.down;
            case Direction.PositiveZ: return Vector3Int.forward;
            case Direction.NegativeZ: default: return Vector3Int.back;
        }
    }
    public static Direction ToDirection(this Vector3Int self) {
        return ((Vector3) self).ToDirection();
    }
    public static Direction Rotate(this Direction self, int quarterTurns, Direction axis) {
        quarterTurns = (quarterTurns%4 + 4)%4;
        if (axis == self || axis == self.Opposite()) {
            return self;
        } else switch (quarterTurns) {
            case 1:
                switch (self) {
                    case Direction.NegativeX: case Direction.NegativeY: case Direction.NegativeZ:
                        return self.Opposite().Rotate(1, axis).Opposite();
                }
                switch (axis) {
                    case Direction.NegativeX: case Direction.NegativeY: case Direction.NegativeZ:
                        return self.Rotate(1, axis.Opposite()).Opposite();
                }
                switch (self) {
                    case Direction.PositiveX: switch (axis) {
                        case Direction.PositiveY: return Direction.NegativeZ;
                        case Direction.PositiveZ: default: return Direction.PositiveY;
                    }
                    case Direction.PositiveY: switch (axis) {
                        case Direction.PositiveX: return Direction.PositiveZ;
                        case Direction.PositiveZ: default: return Direction.NegativeX;
                    }
                    case Direction.PositiveZ: default: switch (axis) {
                        case Direction.PositiveX: return Direction.NegativeY;
                        case Direction.PositiveY: default: return Direction.PositiveX;
                    }
                }
            case 2: return self.Opposite();
            case 3: return self.Rotate(1, axis).Opposite();
            case 0: default: return self;
        }
    }
    public static Direction Opposite(this Direction self) {
        switch (self) {
            case Direction.PositiveX: return Direction.NegativeX;
            case Direction.NegativeX: return Direction.PositiveX;
            case Direction.PositiveY: return Direction.NegativeY;
            case Direction.NegativeY: return Direction.PositiveY;
            case Direction.PositiveZ: return Direction.NegativeZ;
            case Direction.NegativeZ: default: return Direction.PositiveZ;
        }
    }
    public static Vector3Int Rotate(this Vector3Int self, int quarterTurns, Direction axis) {
        quarterTurns = (quarterTurns%4 + 4)%4;
        if (quarterTurns == 0) {
            return self;
        } else switch (axis) {
            case Direction.NegativeX: case Direction.NegativeY: case Direction.NegativeZ:
                return self.Rotate(quarterTurns, axis.Opposite());
            case Direction.PositiveX: switch (quarterTurns) {
                case 1: return new Vector3Int(self.x, -self.z, self.y);
                case 2: return new Vector3Int(self.x, -self.y, -self.z);
                case 3: default: return new Vector3Int(self.x, self.z, -self.y);
            }
            case Direction.PositiveY: switch (quarterTurns) {
                case 1: return new Vector3Int(self.z, self.y, -self.x);
                case 2: return new Vector3Int(-self.x, self.y, -self.z);
                case 3: default: return new Vector3Int(-self.z, self.y, self.x);
            }
            case Direction.PositiveZ: default: switch (quarterTurns) {
                case 1: return new Vector3Int(-self.y, self.x, self.z);
                case 2: return new Vector3Int(-self.x, -self.y, self.z);
                case 3: default: return new Vector3Int(self.y, -self.x, self.z);
            }
        }
    }
    public static Vector3 Rotate(this Vector3 self, int quarterTurns, Direction axis) {
        quarterTurns = (quarterTurns%4 + 4)%4;
        if (quarterTurns == 0) {
            return self;
        } else switch (axis) {
                case Direction.NegativeX: case Direction.NegativeY: case Direction.NegativeZ:
                    return self.Rotate(quarterTurns, axis.Opposite());
                case Direction.PositiveX: switch (quarterTurns) {
                case 1: return new Vector3(self.x, -self.z, self.y);
                case 2: return new Vector3(self.x, -self.y, -self.z);
                case 3: default: return new Vector3(self.x, self.z, -self.y);
                }
                case Direction.PositiveY: switch (quarterTurns) {
                case 1: return new Vector3(self.z, self.y, -self.x);
                case 2: return new Vector3(-self.x, self.y, -self.z);
                case 3: default: return new Vector3(-self.z, self.y, self.x);
                }
                case Direction.PositiveZ: default: switch (quarterTurns) {
                case 1: return new Vector3(-self.y, self.x, self.z);
                case 2: return new Vector3(-self.x, -self.y, self.z);
                case 3: default: return new Vector3(self.y, -self.x, self.z);
                }
        }
    }
}
