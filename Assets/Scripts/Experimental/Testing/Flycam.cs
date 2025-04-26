using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class Flycam : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 50.0f;
    [SerializeField] private float lookSpeed = 5.0f;
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    void Awake() {
        inputActions = InputModeManager.Instance.inputActions;
    }
    void OnEnable() {
        inputActions.Player.Move.performed += HandleMoveInputBegin;
        inputActions.Player.Move.canceled += HandleMoveInputEnd;
        inputActions.Player.Look.performed += HandleLookInputBegin;
        inputActions.Player.Look.canceled += HandleLookInputEnd;
    }
    void OnDisable() {
        inputActions.Player.Move.performed -= HandleMoveInputBegin;
        inputActions.Player.Move.canceled -= HandleMoveInputEnd;
        inputActions.Player.Look.performed -= HandleLookInputBegin;
        inputActions.Player.Look.canceled -= HandleLookInputEnd;
    }
    void HandleMoveInputBegin(InputAction.CallbackContext ctx) {
        moveInput = ctx.ReadValue<Vector2>();
    }
    void HandleMoveInputEnd(InputAction.CallbackContext dontCare) {
        moveInput = Vector2.zero;
    }
    void HandleLookInputBegin(InputAction.CallbackContext ctx) {
        lookInput = ctx.ReadValue<Vector2>();
    }
    void HandleLookInputEnd(InputAction.CallbackContext dontCare) {
        lookInput = Vector2.zero;
    }
    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update() {
        transform.Rotate(
            (lookInput.x*Vector3.up - lookInput.y*Vector3.right) *
            lookSpeed * Time.deltaTime
        );
        transform.Rotate(-Vector3.Project(transform.eulerAngles, Vector3.forward));
        transform.position += (
            (moveInput.x*transform.right + moveInput.y*transform.forward) *
            moveSpeed * Time.deltaTime
        );
    }
}
