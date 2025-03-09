using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class FlyingVehicle : MonoBehaviour
{
    [SerializeField] private Interactable rudderInteractTarget;
    [SerializeField] private Transform rudderHelmSeat; // The seat location for the player
    [SerializeField] private float linearAcceleration = 3.0f;
    [SerializeField] private float verticalAcceleration = 1.0f;
    [SerializeField] private float angularAcceleration = 12.0f;
    [SerializeField] private float brake = 0.01f;
    [SerializeField] private float tiltStrength = 2.0f;
    [SerializeField] private float stabilizeStrength = 1.0f;
    [SerializeField] private float traction = 0.01f;
    [SerializeField] private float bobSpeed = 1.0f;
    [SerializeField] private float bobRange = 1.0f;
    [SerializeField] private HingeJoint rudder;
    [SerializeField] private float rudderSpin = 2000.0f;
    [SerializeField] private float lookSens = 1.0f;
    [SerializeField] private float lookXLimit = 75.0f;
    [SerializeField] private Vector3 lookVantage = new Vector3(0.0f, 20.0f, -35.0f);
    [SerializeField] private float lookVantageMaxZ = -35.0f;
    [SerializeField] private float lookVantageMinZ = -70.0f;
    [SerializeField] private float lookVantageZoomSens = 1.0f;
    private const float lookSpeedMult = 0.1f;
    private InputModeManager inputMan;
    private InputSystem_Actions inputActions;
    private Camera playerCamera = null;
    private Vector3 cameraInitialDisplacement;
    private Transform sceneCore; // return the player here after un-parenting
    private Vector2 xzMovementInput = Vector2.zero;
    private float yMovementInput = 0.0f;
    private Vector2 lookInput = Vector2.zero;
    private float zoomInput = 0.0f;
    private Vector3 impetus = Vector3.zero;
    private float baseY;
    private float bobDirection = -1.0f;
    private float targetY;
    private Rigidbody rbody;
    private GameObject currentPlayer = null;

    // events
    private System.Action<InputAction.CallbackContext> movePerformedAction;
    private System.Action<InputAction.CallbackContext> moveCanceledAction;
    private System.Action<InputAction.CallbackContext> lookPerformedAction;
    private System.Action<InputAction.CallbackContext> lookCanceledAction;
    private System.Action<InputAction.CallbackContext> zoomPerformedAction;
    private System.Action<InputAction.CallbackContext> zoomCanceledAction;
    private System.Action<InputAction.CallbackContext> interactPerformedAction;
    private System.Action<InputAction.CallbackContext> jumpOffPerformedAction;
    private System.Action<InputAction.CallbackContext> ascendPerformedAction;
    private System.Action<InputAction.CallbackContext> ascendCanceledAction;
    private System.Action<InputAction.CallbackContext> descendPerformedAction;
    private System.Action<InputAction.CallbackContext> descendCanceledAction;

    void Awake()
    {
        inputMan = InputModeManager.Instance;
        inputActions = InputModeManager.Instance.inputActions;
    }
    void OnEnable()
    {
        rudderInteractTarget.OnInteract += doRudderInteraction;

        // Bind actions to methods and store them
        movePerformedAction = ctx => xzMovementInput = ctx.ReadValue<Vector2>();
        moveCanceledAction = ctx => xzMovementInput = Vector2.zero;
        lookPerformedAction = ctx => lookInput = ctx.ReadValue<Vector2>();
        lookCanceledAction = ctx => lookInput = Vector2.zero;
        zoomPerformedAction = ctx => zoomInput = ctx.ReadValue<float>();
        zoomCanceledAction = ctx => zoomInput = 0.0f;
        interactPerformedAction = ctx => RelinquishFocus(currentPlayer);
        jumpOffPerformedAction = ctx => RelinquishFocus(currentPlayer);
        ascendPerformedAction = ctx => yMovementInput = 1.0f;
        ascendCanceledAction = ctx => yMovementInput = 0.0f;
        descendPerformedAction = ctx => yMovementInput = -1.0f;
        descendCanceledAction = ctx => yMovementInput = 0.0f;

        inputActions.Flying.Move.performed += movePerformedAction;
        inputActions.Flying.Move.canceled += moveCanceledAction;
        inputActions.Flying.Look.performed += lookPerformedAction;
        inputActions.Flying.Look.canceled += lookCanceledAction;
        inputActions.Flying.Zoom.performed += zoomPerformedAction;
        inputActions.Flying.Zoom.canceled += zoomCanceledAction;
        inputActions.Flying.Interact.performed += interactPerformedAction;
        inputActions.Flying.Jump_Off.performed += jumpOffPerformedAction;
        inputActions.Flying.Ascend.performed += ascendPerformedAction;
        inputActions.Flying.Ascend.canceled += ascendCanceledAction;
        inputActions.Flying.Descend.performed += descendPerformedAction;
        inputActions.Flying.Descend.canceled += descendCanceledAction;
    }

    void OnDisable()
    {
        // Unbind actions using stored lambdas
        inputActions.Flying.Move.performed -= movePerformedAction;
        inputActions.Flying.Move.canceled -= moveCanceledAction;
        inputActions.Flying.Look.performed -= lookPerformedAction;
        inputActions.Flying.Look.canceled -= lookCanceledAction;
        inputActions.Flying.Zoom.performed -= zoomPerformedAction;
        inputActions.Flying.Zoom.canceled -= zoomCanceledAction;
        inputActions.Flying.Interact.performed -= interactPerformedAction;
        inputActions.Flying.Jump_Off.performed -= jumpOffPerformedAction;
        inputActions.Flying.Ascend.performed -= ascendPerformedAction;
        inputActions.Flying.Ascend.canceled -= ascendCanceledAction;
        inputActions.Flying.Descend.performed -= descendPerformedAction;
        inputActions.Flying.Descend.canceled -= descendCanceledAction;

        inputActions.Flying.Disable();
        rudderInteractTarget.OnInteract -= doRudderInteraction;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetY = baseY = transform.position.y;
        rbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Operating())
        {
            playerCamera = null;
            impetus = Vector3.zero;
        }
        // Handle movement inputs at current frame
        HandleMovementInput(xzMovementInput, yMovementInput);
        HandleLookInput(lookInput);
        // Move altitude targets if ascending/descending
        if (impetus.y == 0.0f)
        {
            // Bobbing
            targetY += bobDirection*bobSpeed*Time.deltaTime;
            if ((targetY - baseY) / bobDirection > bobRange)
            {
                bobDirection = -bobDirection;
            }
        }
        else
        {
            targetY = transform.position.y;
            baseY = transform.position.y;
        }
        // Apply Forces
        rbody.AddTorque(transform.up * impetus.x * rbody.mass * angularAcceleration * Time.deltaTime, ForceMode.Impulse);
        rbody.AddForce(transform.forward * impetus.z * rbody.mass * linearAcceleration * Time.deltaTime, ForceMode.Impulse);
        rbody.AddForce(Vector3.up * rbody.mass * (targetY - transform.position.y) * bobSpeed * Time.deltaTime, ForceMode.Impulse);
        // Steer
        rbody.linearVelocity = Vector3.Lerp(
            rbody.linearVelocity,
            Vector3.ProjectOnPlane(rbody.linearVelocity, transform.right),
            1.0f - Mathf.Pow(1.0f - traction, Time.deltaTime * 60.0f)
        );
        // Brake
        if (Vector3.Dot(transform.forward*impetus.z, rbody.linearVelocity) < 0.0f)
        {
            rbody.linearVelocity = Vector3.Lerp(
                rbody.linearVelocity,
                -rbody.linearVelocity.normalized,
                1.0f - Mathf.Pow(1.0f - brake, Time.deltaTime * 60.0f)
            );
        }
        // Steering Wheel Animation
        if (rudder)
        {
            var motor = rudder.motor;
            motor.targetVelocity = rbody.angularVelocity.y * rudderSpin;
            rudder.motor = motor;
        }
        if (impetus.z == 0.0)
        {
            // If ascending or descending without forward or backward motion, rise or drop in-place
            rbody.AddForce(transform.up*impetus.y*rbody.mass*verticalAcceleration*Time.deltaTime, ForceMode.Impulse);
        }
        else
        {
            // Tilt ship with ascent/descent
            rbody.AddTorque(
                -rbody.mass*angularAcceleration*Time.deltaTime*transform.right*impetus.y *
                tiltStrength *
                Vector3.Dot(transform.up, Vector3.up)/4.0f,
                            ForceMode.Impulse
            );
        }
        // Stabilize
        rbody.AddTorque(
            rbody.mass*angularAcceleration*Time.deltaTime *
                stabilizeStrength *
                Vector3.Cross(transform.up, Vector3.up),
            ForceMode.Impulse
        );
    }

    void doRudderInteraction(GameObject player)
    {
        inputMan.SwitchToShipControls();
        rudderInteractTarget.DisableInteractions();
        var pcm = player.GetComponent<PlayerCharacterController>();
        if (pcm)
        {
            // Snap the player to the rudderHelmSeat position and rotation
            currentPlayer = player;
            currentPlayer.transform.position = rudderHelmSeat.position;
            currentPlayer.transform.SetParent(rudderHelmSeat); // Parent the player to the rudderHelmSeat

            // Grab the camera
            playerCamera = pcm.playerCamera;
            cameraInitialDisplacement = playerCamera.transform.localPosition;
            playerCamera.transform.SetParent(transform);
            HandleLookInput(Vector2.zero); // initialize camera transform

        }
        else
        {
            playerCamera = null;
        }
    }

    private void RelinquishFocus(GameObject player)
    {
        if (playerCamera)
        {
            playerCamera.transform.SetParent(player.transform);
            playerCamera.transform.localPosition = cameraInitialDisplacement;
        }
        player.transform.SetParent(sceneCore); // Unparent the player from the rudderHelmSeat
        player.transform.eulerAngles = new Vector3(
            0.0f, player.transform.eulerAngles.y, 0.0f
        );
        rudderInteractTarget.EnableInteractions();
        inputMan.SwitchToPlayerControls();
    }

    public bool Operating()
    {
        return inputActions.Flying.enabled;
    }

    private void HandleMovementInput(Vector2 xz, float y)
    {
        impetus.x = xz.x;
        impetus.y = y;
        impetus.z = xz.y;
    }

    private void HandleLookInput(Vector2 yx)
    {
        if (playerCamera)
        {
            float pitch = playerCamera.transform.eulerAngles.x;
            float pitchCos = Mathf.Cos(pitch*Mathf.PI/180.0f);
            playerCamera.transform.Rotate(
                transform.up,
                pitchCos*yx.x*lookSens*lookSpeedMult
            );
            float dx = -yx.y*lookSens*lookSpeedMult;
            float checkx = ((pitch%360.0f) + 360.0f)%360.0f;
            if (checkx >= 180.0f)
            {
                checkx -= 360.0f;
            }
            if (dx != 0.0f && checkx/Mathf.Sign(dx) <= lookXLimit)
            {
                playerCamera.transform.Rotate(transform.right, dx);
            }
            playerCamera.transform.eulerAngles =
                Vector3.ProjectOnPlane(playerCamera.transform.eulerAngles, Vector3.forward);
            lookVantage.z += zoomInput*lookVantageZoomSens;
            if (lookVantage.z > lookVantageMaxZ)
            {
                lookVantage.z = lookVantageMaxZ;
            }
            else if (lookVantage.z < lookVantageMinZ)
            {
                lookVantage.z = lookVantageMinZ;
            }
            playerCamera.transform.position = transform.position +
                lookVantage.x*playerCamera.transform.right +
                lookVantage.y*transform.up +
                lookVantage.z*playerCamera.transform.forward;
        }
    }
}
