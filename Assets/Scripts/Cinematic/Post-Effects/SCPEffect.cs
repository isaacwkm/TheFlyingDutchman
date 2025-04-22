using UnityEngine;

public abstract class SCPEffect : MonoBehaviour
{
    public abstract string Name { get; }
    public abstract void PlayForward(float speed);
    public abstract void PlayBackward(float speed);
}