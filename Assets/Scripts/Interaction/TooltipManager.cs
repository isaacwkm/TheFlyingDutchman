using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    private enum CenterScreenWidgets
    {
        None = -1,
        Crosshair = 0,
        PrimaryTooltip = 1,
        SecondaryTooltip = 2

    }

    [Header("Interaction Tooltip References")]
    [SerializeField] private PlayerCharacterController playerController; // reference to player controller script
    [SerializeField] private GameObject cameraObj; // reference to camera obj
    [SerializeField] private TextMeshProUGUI tooltipUiComponent; // Reference to the tooltip UI Text component
    [SerializeField] private TextMeshProUGUI secondTooltipComponent; // Reference to the second tooltip UI Text component
    [SerializeField] private Image crosshair; // Reference to crosshair
    private InputSystem_Actions inputActions; // Reference to the input actions

    // Below need to be initialized in Start()
    private Camera playerCamera; // Reference to the Camera component of the player camera
    private Transform cameraTransform; // Reference to the player's transform
    private float interactionRange; // Range within which interaction is allowed

    // Class variables
    private string tooltipText = "Interact: [Key]"; // Tooltip action text
    private string secondTooltipText = "Interact: [Key]"; // Tooltip action text
    private CenterScreenWidgets currentCenterScreenWidget = CenterScreenWidgets.Crosshair;

    private void Awake()
    {
        // Initialize the input actions
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
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
        if (tooltipUiComponent != null && secondTooltipComponent != null)
        {
            tooltipUiComponent.gameObject.SetActive(false);
            secondTooltipComponent.gameObject.SetActive(false);
        }

    }
    private void Update()
    {
        if (cameraTransform == null) return;

        UpdateCenterScreenWidget();
    }

    private void UpdateCenterScreenWidget() // Runs updates in order of ascending priority: Primary tooltip takes precedence over Secondary tooltip, and Secondary takes precendence over the player's crosshair.
    {
        // Each sequential update overwrites the previous one, if active. Example: If a primary tooltip were to be active, then it overwrites the secondary one (hides secondary)
        CrosshairPeek(); // Show crosshair when playing
        SecondaryPeek();
        RaycastPeek(); // For objects which show a tooltip based on player's camera position (sends out a raycast where they are looking). Is also: Primary Peek
    }

    private void ShowCrosshair()
    {
        if (crosshair != null)
        {
            HideCenterScreenWidgets();
            crosshair.gameObject.SetActive(true);
            currentCenterScreenWidget = CenterScreenWidgets.Crosshair;
        }
    }
    private void ShowTooltip()
    {
        if (tooltipUiComponent != null)
        {
            HideCenterScreenWidgets();
            tooltipUiComponent.gameObject.SetActive(true);
            tooltipUiComponent.text = tooltipText;
            currentCenterScreenWidget = CenterScreenWidgets.PrimaryTooltip;
        }
    }

    private void ShowSecondTooltip()
    {
        if (secondTooltipComponent != null)
        {
            HideCenterScreenWidgets();
            secondTooltipComponent.gameObject.SetActive(true);
            secondTooltipComponent.text = secondTooltipText;
            currentCenterScreenWidget = CenterScreenWidgets.SecondaryTooltip;
        }
    }

    private void HideCenterScreenWidgets()
    {
        crosshair.gameObject.SetActive(false);
        tooltipUiComponent.gameObject.SetActive(false);
        secondTooltipComponent.gameObject.SetActive(false);
        currentCenterScreenWidget = CenterScreenWidgets.None;
    }

    // AKA: PrimaryPeek. Is the highest priority tooltip which prioritizes showing information relevant to what the player is looking at.
    private void RaycastPeek() // For objects which show a tooltip based on player's camera position (sends out a raycast where they are looking)
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            Interactable whom = hit.collider.GetComponent<Interactable>();
            if (whom && whom.isActiveAndEnabled)
            {
                // if (interaction requirements are not met)
                if (!whom.canInteract(gameObject)) // Game object is player here
                {
                    tooltipText = whom.peekRequirementText();
                }
                else
                { // if (interaction requirements are met)
                    tooltipText = whom.peekActionText();
                    tooltipText = appendActionKey(tooltipText); // Append the action key for interact to the tooltip message
                }
                ShowTooltip();
                return;
            }
        }
    }

    private void SecondaryPeek()
    {
        if (currentCenterScreenWidget == CenterScreenWidgets.PrimaryTooltip) return; // Do nothing if the primary tooltip is on-screen.

        ShowSecondTooltip();
    }

    private void CrosshairPeek() // Okay this method isn't actually peeking into anything, but it's named peek to be in line with the other methods in its group semantically.
    {
        // Checks if crosshair is allowed to be drawn on screen
        if (InputModeManager.Instance.inputMode != InputModeManager.InputMode.Player) return; // Do nothing if the player isn't in first person movement mode

        ShowCrosshair();
    }

    private string appendActionKey(string toolTipText)
    {
        return $"{tooltipText}: [{inputActions.Player.Interact.bindings[0].ToDisplayString()}]"; // Display the action key
    }

}
