using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Needle.Console;
using System.Collections;

[SelectionBase] // Automatically select the parent when its child is selected
[RequireComponent(typeof(CharacterController))]
public class PlayerCharacterController : MonoBehaviour
{
    private enum MovementMode
    {
        Normal,
        Ladder
    }

    public Camera playerCamera;
    public Inventory inventoryComponent;
    public ActionSound SuperjumpSound;
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float climbSpeed = 6f;
    [SerializeField] private float jumpPower = 7f;
    [SerializeField] private float superJumpPower = 20f;
    [SerializeField] private float gravity = 10f;
    [SerializeField] private float lookSens = 1f;
    private const float lookSpeedMult = 0.1F; // Constant multipler to make looking around feel natural at lookspeed = 1 (default sensitivity setting for new players);
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
    private Action<InputAction.CallbackContext> onJumpPerformed;
    private Action<InputAction.CallbackContext> onJumpCancelled;
    private Action<InputAction.CallbackContext> onSuperJumpPerformed;
    private Action<InputAction.CallbackContext> onSuperJumpCancelled;
    private Action<InputAction.CallbackContext> onCrouchPerformed;
    private Action<InputAction.CallbackContext> onCrouchCancelled;
    private bool isJumping = false;
    private bool isCrouching = false;
    private bool isChargingSuperJump = false;
    private float superJumpCharge = 0;
    private bool wasGrounded = false;
    private bool justLanded = false;

    private MovementMode movementMode = MovementMode.Normal;
    private GameObject movementMedium = null; // Represents the gameObject that the player is using to access a special mode of movement (?) - Isaac

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        inputActions = InputModeManager.Instance.inputActions;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        // Store Delegates
        onJumpPerformed = ctx => isJumping = true;
        onJumpCancelled = ctx => isJumping = false;
        onSuperJumpPerformed = ctx => SuperJumpCharging();
        onSuperJumpCancelled = ctx => SuperJumpReleased();
        onCrouchPerformed = ctx => isCrouching = true;
        onCrouchCancelled = ctx => isCrouching = false;

        // Bind actions to methods
        inputActions.Player.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => movementInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        enableJumping(true); // Eventually the goal is to replace all the other event listeners with something like this, but only when that needs comes.
        enableSuperJumping(true);

        enableCrouching(true);

        inputActions.Player.Interact.performed += ctx => HandleInteractInput();
        inputActions.Player.Previous.performed += ctx => HandleItemPrevInput();
        inputActions.Player.Next.performed += ctx => HandleItemNextInput();
        inputActions.Player.Drop.performed += ctx => HandleDropInput();

        inputActions.Player.SwitchTo1.performed += ctx => HandleSwitchToSlotInput(0);
        inputActions.Player.SwitchTo2.performed += ctx => HandleSwitchToSlotInput(1);
        inputActions.Player.SwitchTo3.performed += ctx => HandleSwitchToSlotInput(2);
        inputActions.Player.SwitchTo4.performed += ctx => HandleSwitchToSlotInput(3);

        inputActions.Player.SwitchScroll.performed += ctx => HandleScrollInput(ctx);

        inputActions.Player.Attack.performed += ctx => HandleAttackInput();
    }

    private void OnDisable()
    {
        // Unsubscribe from events to prevent memory leaks or unwanted behavior
        inputActions.Player.Move.performed -= ctx => movementInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled -= ctx => movementInput = Vector2.zero;

        inputActions.Player.Look.performed -= ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled -= ctx => lookInput = Vector2.zero;

        enableJumping(false);
        enableSuperJumping(false);

        enableCrouching(false);

        inputActions.Player.Interact.performed -= ctx => HandleInteractInput();
        inputActions.Player.Previous.performed -= ctx => HandleItemPrevInput();
        inputActions.Player.Next.performed -= ctx => HandleItemNextInput();
        inputActions.Player.Drop.performed -= ctx => HandleDropInput();

        inputActions.Player.SwitchTo1.performed -= ctx => HandleSwitchToSlotInput(0);
        inputActions.Player.SwitchTo2.performed -= ctx => HandleSwitchToSlotInput(1);
        inputActions.Player.SwitchTo3.performed -= ctx => HandleSwitchToSlotInput(2);
        inputActions.Player.SwitchTo4.performed -= ctx => HandleSwitchToSlotInput(3);

        inputActions.Player.SwitchScroll.performed -= ctx => HandleScrollInput(ctx);

        inputActions.Player.Attack.performed -= ctx => HandleAttackInput();

        inputActions.Player.Disable();
    }


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (movementMode == MovementMode.Normal)
        {
            UpdateNormal();
        }
        else if (movementMode == MovementMode.Ladder)
        {
            UpdateOnLadder();
        }
    }

    void UpdateNormal()
    {
        MovePlayer();
        HandleSuperJumpCharge();
        if (InputModeManager.Instance?.inputMode == InputModeManager.InputMode.Player)
        {
            HandleCameraRotation();
        }
        justLanded = (
            characterController.isGrounded && !wasGrounded &&
            Mathf.Abs(characterController.velocity.y) >= 0.1f
        );
        wasGrounded = characterController.isGrounded;
    }

    void UpdateOnLadder()
    {
        MovePlayerOnLadder();
        if (InputModeManager.Instance?.inputMode == InputModeManager.InputMode.Player)
        {
            HandleCameraRotation();
        }
        justLanded = false;
        wasGrounded = true;
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

    private void MovePlayerOnLadder()
    {
        if (movementMedium)
        {
            // climb ladder
            Vector3 movement = Time.deltaTime * movementMedium.transform.up * climbSpeed;
            // if it is a rope
            bool isRope = !!movementMedium.GetComponent<Rope>();
            if (isRope)
            {
                // climbing direction does not depend on rope facing (rope has radial symmetry)
                movement *= movementInput.y;
                // snap to rope
                movement += Vector3.ProjectOnPlane(
                    movementMedium.transform.position - transform.position,
                    movementMedium.transform.up
                );
            }
            else
            {
                // if it isn't a rope, then climbing direction depends on ladder facing
                movement *= Vector3.Dot(
                    -(transform.forward * movementInput.y).normalized,
                    movementMedium.transform.forward
                );
            }
            var collFlags = characterController.Move(movement);
            if ((collFlags & CollisionFlags.Below) != 0)
            {
                RestoreMovementMode();
            }
            else if (isJumping)
            {
                // handle jumping off
                // first, displace away from ladder to prevent jump from landing right back onto it instantly
                if (isRope)
                {
                    characterController.Move(0.5f*transform.forward);
                }
                else
                {
                    characterController.Move(0.5f*movementMedium.transform.forward);
                }
                // then, jump
                RestoreMovementMode();
                moveDirection.y = jumpPower;
            }
        }
    }

    public void RestoreMovementMode()
    {
        movementMode = MovementMode.Normal;
        movementMedium = null;
    }

    public void AttachToLadder(GameObject ladder)
    {
        moveDirection = Vector3.zero;
        movementMode = MovementMode.Ladder;
        movementMedium = ladder;
    }

    public void DetachFromLadder()
    {
        if (movementMode == MovementMode.Ladder)
        {
            movementMode = MovementMode.Normal;
            movementMedium = null;
            characterController.Move(0.5f * Vector3.up);
        }
    }

    public bool AttachedTo(GameObject medium)
    {
        return movementMedium == medium;
    }

    public GameObject GetMovementMedium()
    {
        return movementMedium;
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

    private void HandleScrollInput(InputAction.CallbackContext context)
    {
        float scrollValue = context.ReadValue<float>();

        if (scrollValue > 0)
        {
            HandleItemPrevInput(); // Scroll up (Prev Item)
        }
        else if (scrollValue < 0)
        {
            HandleItemNextInput(); // Scroll down (Next Item)
        }
    }
    private void HandleItemPrevInput()
    {
        D.Log("Prev item pressed", null, "Inv");
        inventoryComponent.switchToPrev();
    }

    private void HandleItemNextInput()
    {
        D.Log("Next item pressed", null, "Inv");
        inventoryComponent.switchToNext();
    }

    private void HandleDropInput()
    {
        D.Log("Drop pressed", null, "Inv");
        inventoryComponent.dropItem();
    }

    private void HandleSwitchToSlotInput(int slotNum)
    {
        inventoryComponent.switchToSlot(slotNum);
    }

    private void HandleAttackInput()
    {
        inventoryComponent.attackWithActiveItem();
    }

    private void HandleSuperJumpCharge()
    {
        if (isChargingSuperJump)
        {
            superJumpCharge += Time.deltaTime;
            if (superJumpCharge > 3f) // Cap charge at 3 seconds
            {
                SuperJumpReleased();
                return;
            }
            else if (superJumpCharge > 0.2f && !characterController.isGrounded) // Release charge if player falls of a ledge. Accounts for small delay where the player is airborne while in their crouching animation.
            {
                D.Log("Releasing Superjump due to player falling off!", gameObject, "Move");
                D.Log($"isGrounded: {characterController.isGrounded}", gameObject, "Move");
                SuperJumpReleased();
                return;
            }


        }
    }
    private void SuperJumpCharging()
    {
        if (!characterController.isGrounded && superJumpCharge == 0) return; // Don't start charging if player isn't grounded

        // Disable jumping and crouching while charging SuperJump
        inputActions.Player.Crouch.performed -= ctx => isCrouching = true;
        inputActions.Player.Crouch.canceled -= ctx => isCrouching = false;
        enableJumping(false);
        isCrouching = true;
        isChargingSuperJump = true;
        superJumpCharge += Time.deltaTime;

    }

    private void SuperJumpReleased()
    {
        if (superJumpCharge == 0) return; // Don't do anything if the player pressed & released the superjump button without the correct

        // Re-enable jumping and crouching while charging SuperJump
        inputActions.Player.Crouch.performed += ctx => isCrouching = true;
        inputActions.Player.Crouch.canceled += ctx => isCrouching = false;
        enableJumping(true);

        if (superJumpCharge > 3f) // Cap charge at 3 seconds
        {
            superJumpCharge = 3f;
        }

        float superJumpPowerMultiplier = 1f + (superJumpCharge * 0.2f); // Convert charge time to percent: 1s = 20%

        SuperjumpSound.PlaySingleRandom(); //play sound effect
        moveDirection.y = superJumpPower * superJumpPowerMultiplier;

        D.Log($"SuperJumped! Charge time: {superJumpCharge}", gameObject, "Move");
        isCrouching = false;
        isChargingSuperJump = false;
        superJumpCharge = 0;
    }

    // Event Listener Enablers (and Disablers)
    public void enableJumping(bool active)
    {
        if (active)
        {
            enableJumping(false); // unsub first to avoid duplicate event listeners.

            inputActions.Player.Jump.performed += onJumpPerformed;
            inputActions.Player.Jump.canceled += onJumpCancelled;
        }
        else
        {
            inputActions.Player.Jump.performed -= onJumpPerformed;
            inputActions.Player.Jump.canceled -= onJumpCancelled;
            isJumping = false;
        }
    }

    public void enableSuperJumping(bool active)
    {
        if (active)
        {
            enableSuperJumping(false); // unsub first to avoid duplicate event listeners.

            inputActions.Player.SuperJump.performed += onSuperJumpPerformed;
            inputActions.Player.SuperJump.canceled += onSuperJumpCancelled;
        }
        else
        {
            inputActions.Player.SuperJump.performed -= onSuperJumpPerformed;
            inputActions.Player.SuperJump.canceled -= onSuperJumpCancelled;
            isChargingSuperJump = false;
        }
    }

    public void enableCrouching(bool active)
    {
        if (active)
        {
            enableCrouching(false); // unsub first to avoid duplicate event listeners.

            inputActions.Player.Crouch.performed += onCrouchPerformed;
            inputActions.Player.Crouch.canceled += onCrouchCancelled;
        }
        else
        {
            inputActions.Player.Crouch.performed -= onCrouchPerformed;
            inputActions.Player.Crouch.canceled -= onCrouchCancelled;
            isCrouching = false;
        }
    }


    // Getters & Setters

    public float getMaxInteractDistance()
    {
        return maxInteractDistance;
    }

    public bool AnyMovementInput()
    {
        return movementInput != Vector2.zero;
    }

    public bool JustLanded()
    {
        return justLanded;
    }

    public bool Climbing()
    {
        return movementMode == MovementMode.Ladder;
    }
}
