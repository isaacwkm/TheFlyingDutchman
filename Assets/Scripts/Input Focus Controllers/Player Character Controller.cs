using System;
using UnityEngine;
using UnityEngine.InputSystem;

[SelectionBase] // Automatically select the parent when its child is selected
[RequireComponent(typeof(CharacterController))]
public class PlayerCharacterController : MonoBehaviour
{
    public Camera playerCamera;
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float jumpPower = 7f;
    [SerializeField] private float gravity = 10f;
    [SerializeField] private float lookSens = 1f;
    private const float lookSpeedMult = 0.1F; // Constant multipler to make looking around feel natural at lookspeed = 1 (default setting for new players);
    [SerializeField] private float lookXLimit = 90f;
    [SerializeField] private float defaultHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchSpeed = 3f;
    [SerializeField] private float maxInteractDistance = 2f;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;

    private InputSystem_Actions inputActions;
    private Vector2 movementInput;
    private Vector2 lookInput;
    private bool isJumping = false;
    private bool isCrouching = false;

    private InputAction.CallbackContext movementInputCtx; // Added to store delegate

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        inputActions = InputModeManager.Instance.inputActions;
    }

private void OnEnable()
{
    inputActions.Player.Enable();

    // Bind actions to methods
    inputActions.Player.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
    inputActions.Player.Move.canceled += ctx => movementInput = Vector2.zero;

    inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
    inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

    inputActions.Player.Jump.performed += ctx => isJumping = true;
    inputActions.Player.Jump.canceled += ctx => isJumping = false;

    inputActions.Player.Crouch.performed += ctx => isCrouching = true;
    inputActions.Player.Crouch.canceled += ctx => isCrouching = false;

    inputActions.Player.Interact.performed += ctx => HandleInteractInput();
    inputActions.Player.Previous.performed += ctx => HandleItemPrevInput();
    inputActions.Player.Next.performed += ctx => HandleItemNextInput();
}

private void OnDisable()
{
    // Unsubscribe from events to prevent memory leaks or unwanted behavior
    inputActions.Player.Move.performed -= ctx => movementInput = ctx.ReadValue<Vector2>();
    inputActions.Player.Move.canceled -= ctx => movementInput = Vector2.zero;

    inputActions.Player.Look.performed -= ctx => lookInput = ctx.ReadValue<Vector2>();
    inputActions.Player.Look.canceled -= ctx => lookInput = Vector2.zero;

    inputActions.Player.Jump.performed -= ctx => isJumping = true;
    inputActions.Player.Jump.canceled -= ctx => isJumping = false;

    inputActions.Player.Crouch.performed -= ctx => isCrouching = true;
    inputActions.Player.Crouch.canceled -= ctx => isCrouching = false;

    inputActions.Player.Interact.performed -= ctx => HandleInteractInput();
    inputActions.Player.Previous.performed -= ctx => HandleItemPrevInput();
    inputActions.Player.Next.performed -= ctx => HandleItemNextInput();

    inputActions.Player.Disable();
}


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        MovePlayer();
        HandleCameraRotation();
    }

    private void MovePlayer()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = movementInput.x * (isCrouching ? crouchSpeed : walkSpeed);
        float curSpeedY = movementInput.y * (isCrouching ? crouchSpeed : walkSpeed);

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
    }

    private void HandleCameraRotation()
    {
        float lookSpeed = lookSens * lookSpeedMult;
        rotationX += -lookInput.y * lookSens * lookSpeedMult;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, lookInput.x * lookSens * lookSpeedMult, 0);
    }

    private void HandleInteractInput()
    {
        //Debug.Log("HandleInteractInput()");
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, maxInteractDistance))
        {
            Interactable whom = hit.collider.GetComponent<Interactable>();
            if (whom)
            {
                whom.receiveInteract(this.gameObject);
            }
        }
    }

    private void HandleItemPrevInput()
    {
        Debug.Log("Previous item selected");
    }

    private void HandleItemNextInput()
    {
        Debug.Log("Next item selected");
    }


    // Getters & Setters

    public float getMaxInteractDistance()
    {
        return maxInteractDistance;
    }
}
