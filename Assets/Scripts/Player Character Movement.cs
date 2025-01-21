// This file moves the player, lets the player jump, fall, and look around.
// This file is coupled with the Input Event Dispatcher script, which notifies this script to execute the desired movement.
// In the observer pattern, this script is the subscriber. One of the publishers this script is subscribed to is the Input Event Dispatcher Script.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacterMovement : MonoBehaviour
{
    public Camera playerCamera;
    [SerializeField] private float walkSpeed = 6f;
    //[SerializeField] private float runSpeed = 12f;
    [SerializeField] private float jumpPower = 7f;
    [SerializeField] private float gravity = 10f;
    [SerializeField] private float lookSpeed = 2f;
    [SerializeField] private float lookXLimit = 90f;
    [SerializeField] private float defaultHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchSpeed = 3f;
    [SerializeField] private float maxInteractDistance = 3f;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;

    private Vector2 currentMovementInput;
    private bool isJumping = false;
    private bool isCrouching = false;

    private void OnEnable()
    {
        InputEventDispatcher.OnMovementInput += HandleMovementInput;
        InputEventDispatcher.OnJumpInput += HandleJumpInput;
        InputEventDispatcher.OnCrouchInput += HandleCrouchInput;
        InputEventDispatcher.OnInteractInput += HandleInteractInput;
    }

    private void OnDisable()
    {
        InputEventDispatcher.OnMovementInput -= HandleMovementInput;
        InputEventDispatcher.OnJumpInput -= HandleJumpInput;
        InputEventDispatcher.OnCrouchInput -= HandleCrouchInput;
        InputEventDispatcher.OnInteractInput -= HandleInteractInput;
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = 0.0f;
        float curSpeedY = 0.0f;
        if (InputEventDispatcher.holdsInputFocus(this)) {
            curSpeedY = currentMovementInput.y * (isCrouching ? crouchSpeed : walkSpeed);
            curSpeedX = currentMovementInput.x * (isCrouching ? crouchSpeed : walkSpeed);
        }
        float movementDirectionY = moveDirection.y;
        moveDirection = (right * curSpeedX) + (forward * curSpeedY);

        if (isJumping && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.height = isCrouching ? crouchHeight : defaultHeight;
        characterController.Move(moveDirection * Time.deltaTime);

        // Camera rotation
        if (InputEventDispatcher.holdsInputFocus(this)) {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    private void HandleMovementInput(Vector2 movement)
    {
        if (InputEventDispatcher.holdsInputFocus(this)) {
            currentMovementInput = movement;
        } else {
            currentMovementInput = Vector2.zero;
        }
    }

    private void HandleJumpInput(bool jump)
    {
        if (InputEventDispatcher.holdsInputFocus(this)) {
            isJumping = jump;
        } else {
            isJumping = false;
        }
    }

    private void HandleCrouchInput(bool crouch)
    {
        if (InputEventDispatcher.holdsInputFocus(this)) {
            isCrouching = crouch;
        } else {
            isCrouching = false;
        }
    }

    private void HandleInteractInput(bool interact) {
        if (interact && InputEventDispatcher.holdsInputFocus(this)) {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.distance <= maxInteractDistance) {
                Interactable whom = hit.collider.GetComponent<Interactable>();
                if (whom) {
                    whom.receiveInteract(this.gameObject);
                }
            }
        }
    }
}
