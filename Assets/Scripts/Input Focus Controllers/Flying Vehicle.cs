using System;
using UnityEngine;

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
    private Vector3 impetus = Vector3.zero;
    private float baseY;
    private float bobDirection = -1.0f;
    private Rigidbody rbody;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }
    void OnEnable()
    {
        inputActions.Enable();
        rudderInteractTarget.OnInteract += doRudderInteraction;

        // Bind actions to methods
        //inputActions.Flying.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        //inputActions.Flying.Move.canceled += ctx => movementInput = Vector2.zero;
        inputActions.Flying.Interact.performed += ctx => RelinquishFocus();
        inputActions.Flying.Jump_Off.performed += ctx => RelinquishFocus();
    }

    void OnDisable()
    {
        inputActions.Disable();
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
        if ((transform.position.y - baseY) / bobDirection > bobRange)
        {
            bobDirection = -bobDirection;
        }
        rbody.AddTorque(transform.up * impetus.x * rbody.mass * angularAcceleration * Time.deltaTime, ForceMode.Impulse);
        rbody.AddForce(transform.forward * impetus.z * rbody.mass * linearAcceleration * Time.deltaTime, ForceMode.Impulse);
        rbody.AddForce(Vector3.up * rbody.mass * bobSpeed * bobDirection * Time.deltaTime, ForceMode.Impulse);
        rbody.linearVelocity = Vector3.Lerp(
            rbody.linearVelocity,
            Vector3.ProjectOnPlane(rbody.linearVelocity, transform.right),
            1.0f - Mathf.Pow(1.0f - traction, Time.deltaTime * 60.0f)
        );
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