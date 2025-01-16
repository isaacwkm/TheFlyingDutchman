using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacterMovement : MonoBehaviour
{
    public Camera playerCamera;
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 12f;
    [SerializeField] private float jumpPower = 7f;
    [SerializeField] private float gravity = 10f;
    [SerializeField] private float lookSpeed = 2f;
    [SerializeField] private float lookXLimit = 90f;
    [SerializeField] private float defaultHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchSpeed = 3f;

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
    }

    private void OnDisable()
    {
        InputEventDispatcher.OnMovementInput -= HandleMovementInput;
        InputEventDispatcher.OnJumpInput -= HandleJumpInput;
        InputEventDispatcher.OnCrouchInput -= HandleCrouchInput;
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

        float curSpeedX = currentMovementInput.y * (isCrouching ? crouchSpeed : walkSpeed);
        float curSpeedY = currentMovementInput.x * (isCrouching ? crouchSpeed : walkSpeed);
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

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
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
    }

    private void HandleMovementInput(Vector2 movement)
    {
        currentMovementInput = movement;
    }

    private void HandleJumpInput(bool jump)
    {
        isJumping = jump;
    }

    private void HandleCrouchInput(bool crouch)
    {
        isCrouching = crouch;
    }
}