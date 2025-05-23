using System;
using UnityEngine;

[RequireComponent(typeof(HingeJoint))]
public class Door : MonoBehaviour
{
    [SerializeField] private Interactable interactTarget;
    private HingeJoint hjoint;
    private bool isOpen;

    public bool InteractRequirementsMet()
    {
        return true;
    }
    void OnEnable()
    {
        interactTarget.OnInteract += OpenOrClose;
    }

    void OnDisable()
    {
        interactTarget.OnInteract -= OpenOrClose;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hjoint = GetComponent<HingeJoint>();
    }

    void OpenOrClose(GameObject dontCare = null)
    {
        var motor = hjoint.motor;
        motor.targetVelocity *= -1.0f;
        hjoint.motor = motor;
    }
}
