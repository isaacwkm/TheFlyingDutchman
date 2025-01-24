using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class FlyingVehicle : MonoBehaviour
{
    [SerializeField] private Interactable rudderInteractTarget;
    [SerializeField] private InputModeManager inputMode;
    [SerializeField] private float linearAcceleration = 3.0f;
    [SerializeField] private float angularAcceleration = 12.0f;
    [SerializeField] private float traction = 0.75f;
    [SerializeField] private float bobSpeed = 1.0f;
    [SerializeField] private float bobRange = 1.0f;
    [SerializeField] private float vantage = 1.5f;
    [SerializeField] private HingeJoint rudder;
    [SerializeField] private float rudderSpin = 20.0f;
    private InputSystem_Actions inputActions;
    private Camera playerCamera = null;
    private Vector3 cameraInitialDisplacement;
    private Vector2 xzMovementInput = Vector2.zero;
    private Vector3 impetus = Vector3.zero;
    private float baseY;
    private float bobDirection = -1.0f;
    private Rigidbody rbody;

    // events
    private System.Action<InputAction.CallbackContext> movePerformedAction;
    private System.Action<InputAction.CallbackContext> moveCanceledAction;
    private System.Action<InputAction.CallbackContext> interactPerformedAction;
    private System.Action<InputAction.CallbackContext> jumpOffPerformedAction;

    void Awake()
    {
        inputActions = InputModeManager.Instance.inputActions;
    }
    void OnEnable()
    {
        rudderInteractTarget.OnInteract += doRudderInteraction;

        inputActions.Flying.Enable();

        // Bind actions to methods and store them
        movePerformedAction = ctx => xzMovementInput = ctx.ReadValue<Vector2>();
        moveCanceledAction = ctx => xzMovementInput = Vector2.zero;
        interactPerformedAction = ctx => RelinquishFocus();
        jumpOffPerformedAction = ctx => RelinquishFocus();

        inputActions.Flying.Move.performed += movePerformedAction;
        inputActions.Flying.Move.canceled += moveCanceledAction;
        inputActions.Flying.Interact.performed += interactPerformedAction;
        inputActions.Flying.Jump_Off.performed += jumpOffPerformedAction;
    }

    void OnDisable()
    {
        // Unbind actions using stored lambdas
        inputActions.Flying.Move.performed -= movePerformedAction;
        inputActions.Flying.Move.canceled -= moveCanceledAction;
        inputActions.Flying.Interact.performed -= interactPerformedAction;
        inputActions.Flying.Jump_Off.performed -= jumpOffPerformedAction;

        inputActions.Flying.Disable();
        rudderInteractTarget.OnInteract -= doRudderInteraction;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        baseY = transform.position.y;
        rbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Operating())
        {
            if (playerCamera)
            {
                playerCamera.transform.rotation = transform.rotation;
            }
        }
        else
        {
            playerCamera = null;
            impetus = Vector3.zero;
        }
        // Bobbing
        if ((transform.position.y - baseY) / bobDirection > bobRange)
        {
            bobDirection = -bobDirection;
        }
        // Handle movement inputs at current frame
        HandleMovementInput(xzMovementInput);
        // Apply Forces
        rbody.AddTorque(transform.up * impetus.x * rbody.mass * angularAcceleration * Time.deltaTime, ForceMode.Impulse);
        rbody.AddForce(transform.forward * impetus.z * rbody.mass * linearAcceleration * Time.deltaTime, ForceMode.Impulse);
        rbody.AddForce(Vector3.up * rbody.mass * bobSpeed * bobDirection * Time.deltaTime, ForceMode.Impulse);
        rbody.linearVelocity = Vector3.Lerp(
            rbody.linearVelocity,
            Vector3.ProjectOnPlane(rbody.linearVelocity, transform.right),
            1.0f - Mathf.Pow(1.0f - traction, Time.deltaTime * 60.0f)
        );
        // Steering Wheel Animation
        if (rudder)
        {
            var motor = rudder.motor;
            motor.targetVelocity = rbody.angularVelocity.y * rudderSpin;
            rudder.motor = motor;
        }
    }

    void doRudderInteraction(GameObject player)
    {
        inputMode.SwitchToShipControls();
        var pcm = player.GetComponent<PlayerCharacterController>();
        if (pcm)
        {
            playerCamera = pcm.playerCamera;
            cameraInitialDisplacement = playerCamera.transform.localPosition;
            playerCamera.transform.localPosition += Vector3.up * vantage;
        }
        else
        {
            playerCamera = null;
        }
    }

    void RelinquishFocus()
    {
        if (playerCamera)
        {
            playerCamera.transform.localPosition = cameraInitialDisplacement;
        }
        inputMode.SwitchToPlayerControls();
    }

    public bool Operating()
    {
        return inputActions.Flying.enabled;
    }

    void HandleMovementInput(Vector2 movement)
    {
        impetus.x = movement.x;
        impetus.z = movement.y;
    }
}