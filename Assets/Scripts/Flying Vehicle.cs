using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FlyingVehicle : MonoBehaviour
{
    [SerializeField] private Interactable interactTarget;
    [SerializeField] private float linearAcceleration = 1.0f;
    [SerializeField] private float angularAcceleration = 12.0f;
    [SerializeField] private float traction = 0.75f;
    [SerializeField] private float bobSpeed = 1.0f;
    [SerializeField] private float bobRange = 0.1f;
    [SerializeField] private float vantage = 1.5f;
    private Camera playerCamera = null;
    private Vector3 cameraInitialDisplacement;
    private Vector3 impetus = Vector3.zero;
    private float baseY;
    private float bobDirection = -1.0f;
    private Rigidbody rbody;

    void OnEnable() {
        interactTarget.OnInteract += TryAcquireFocus;
    }

    void OnDisable() {
        interactTarget.OnInteract -= TryAcquireFocus;
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
        if (Operating()) {
            if (playerCamera) {
                playerCamera.transform.rotation = transform.rotation;
            }
        } else {
            playerCamera = null;
            impetus = Vector3.zero;
        }
        if ((transform.position.y - baseY)/bobDirection > bobRange) {
            bobDirection = -bobDirection;
        }
        rbody.AddTorque(transform.up*impetus.x*angularAcceleration*Time.deltaTime, ForceMode.Impulse);
        rbody.AddForce(transform.forward*impetus.z*linearAcceleration*Time.deltaTime, ForceMode.Impulse);
        rbody.AddForce(Vector3.up*bobSpeed*bobDirection*Time.deltaTime, ForceMode.Impulse);
        rbody.linearVelocity = Vector3.Lerp(
            rbody.linearVelocity,
            Vector3.ProjectOnPlane(rbody.linearVelocity, transform.right),
            1.0f - Mathf.Pow(1.0f - traction, Time.deltaTime*60.0f)
        );
    }

    void TryAcquireFocus(GameObject player) {
        if (InputEventDispatcher.acquireInputFocus(this)) {
            InputEventDispatcher.OnInteractInput += RelinquishFocus;
            InputEventDispatcher.OnMovementInput += HandleMovementInput;
            var pcm = player.GetComponent<PlayerCharacterMovement>();
            if (pcm) {
                playerCamera = pcm.playerCamera;
                cameraInitialDisplacement = playerCamera.transform.localPosition;
                playerCamera.transform.localPosition += Vector3.up*vantage;
            } else {
                playerCamera = null;
            }
        }
    }

    void RelinquishFocus(bool really = true) {
        if (really) {
            if (playerCamera) {
                playerCamera.transform.localPosition = cameraInitialDisplacement;
            }
            InputEventDispatcher.OnInteractInput -= RelinquishFocus;
            InputEventDispatcher.OnMovementInput -= HandleMovementInput;
            InputEventDispatcher.relinquishInputFocus(this);
        }
    }

    public bool Operating() {
        return InputEventDispatcher.holdsInputFocus(this);
    }

    void HandleMovementInput(Vector2 movement) {
        impetus.x = movement.x;
        impetus.z = movement.y;
    }
}
