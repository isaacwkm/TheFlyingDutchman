using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    [Header("Interaction Tooltip References")]
    [SerializeField] private PlayerCharacterController playerController; // reference to player controller script
    [SerializeField] private GameObject cameraObj; // reference to camera obj
    [SerializeField] private TextMeshProUGUI UiTextComponent; // Reference to the tooltip UI Text component
    [SerializeField] private Image crosshair; // Reference to crosshair
    private InputSystem_Actions inputActions; // Reference to the input actions

    // Below need to be initialized in Start()
    private Camera playerCamera; // Reference to the Camera component of the player camera
    private Transform cameraTransform; // Reference to the player's transform
    private float interactionRange; // Range within which interaction is allowed

    // Class variables
    private string tooltipText = "Interact: [Key]"; // Tooltip action text

    private void Awake()
    {
        // Initialize the input actions
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable(){
        // Enable the input actions (make sure they're enabled)
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }
    private void Start()
    {
        playerCamera = cameraObj.GetComponent<Camera>(); 
        cameraTransform = cameraObj.transform;
        interactionRange = playerController.getMaxInteractDistance();

        // Disable tooltip text at start
        if (UiTextComponent != null)
        {
            UiTextComponent.gameObject.SetActive(false);
        }

    }
    private void Update()
    {
        if (cameraTransform == null) return;

        peek();
    }

    private void ShowTooltip()
    {
        if (UiTextComponent != null)
        {
            crosshair.gameObject.SetActive(false);
            UiTextComponent.gameObject.SetActive(true);
            UiTextComponent.text = $"{tooltipText}: [{inputActions.Player.Interact.bindings[0].ToDisplayString()}]"; // Display the action key
        }
    }

    private void HideTooltip()
    {
        if (UiTextComponent != null)
        {
            crosshair.gameObject.SetActive(true);
            UiTextComponent.gameObject.SetActive(false);
        }
    }

    private void peek()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            Interactable whom = hit.collider.GetComponent<Interactable>();
            if (whom && whom.isActiveAndEnabled)
            {
                // if (requirements met)
                tooltipText = whom.peekActionText();
                ShowTooltip();
                return;
            }
        }
        HideTooltip();

    }

}
