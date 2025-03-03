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
    [SerializeField] private InputPromptReplacer interactButtonIconReplacer;
    private InputSystem_Actions inputActions; // Reference to the input actions

    // Below need to be initialized in Start()
    private Camera playerCamera; // Reference to the Camera component of the player camera
    private Transform cameraTransform; // Reference to the player's transform
    private float interactionRange; // Range within which interaction is allowed

    // Class variables
    private bool secondTooltipActive = false; // Set from external classes depending on situational game elements this class does not handle. "Active" in this context means if it *should* be drawn, but will not necessarily show unless TooltipManager allows it.
    private CenterScreenWidgets currentCenterScreenWidget = CenterScreenWidgets.Crosshair;
    private bool inputDeviceChanged = true; // Tracks if the center screen widget changes. Flips to true then back to false on the next update frame.
    private string cachedInteractButtonText = ""; // To save on slow_performance calls, uses this cached value if changes weren't made.

    private void Awake()
    {
        // Initialize the input actions
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable() // TODO: Create event in InputModeManager that dispatches upon device being changed. Have this script subscribe to it and flip inputDeviceChanged.
    // Potentially, have the subscribe method perform everything needed to cache the input text and get rid of unnecessasry logic.
    {
        // Enable the input actions (make sure they're enabled)
        inputActions.Enable();
        InputModeManager.Instance.OnInputModeSwitch += DoModeSwitchCleanup;
    }

    private void OnDisable()
    {
        inputActions.Disable();
        InputModeManager.Instance.OnInputModeSwitch -= DoModeSwitchCleanup;
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
            currentCenterScreenWidget = CenterScreenWidgets.PrimaryTooltip;
        }
    }

    private void ShowSecondTooltip()
    {
        if (secondTooltipComponent != null)
        {
            HideCenterScreenWidgets();
            secondTooltipComponent.gameObject.SetActive(true);
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
                string tooltipText = "";
                // if (interaction requirements are not met)
                if (!whom.canInteract(gameObject)) // Game object is player here
                {
                    tooltipText = whom.peekRequirementText();
                }
                else
                { // if (interaction requirements are met)
                    tooltipText = whom.peekActionText();
                    tooltipText = appendInteractActionKey(tooltipText); // Append the action key for interact to the tooltip message
                }
                tooltipUiComponent.text = tooltipText;
                ShowTooltip();
                return;
            }
        }
    }

    private void SecondaryPeek()
    {
        if (currentCenterScreenWidget == CenterScreenWidgets.PrimaryTooltip) return; // Do nothing if the primary tooltip is on-screen.
        if (secondTooltipActive == false) return; // Don't show the secondary tooltip if it's not flagged to show.

        ShowSecondTooltip();
    }

    private void CrosshairPeek() // Okay this method isn't actually peeking into anything, but it's named peek to be in line with the other methods in its group semantically.
    {
        // Checks if crosshair is allowed to be drawn on screen
        if (InputModeManager.Instance.inputMode != InputModeManager.InputMode.Player) return; // Do nothing if the player isn't in first person movement mode

        ShowCrosshair();
    }

    private string appendInteractActionKey(string baseString) // Takes a relevant interact action string like "Dig" or "Pick up" and appends the interact action button binding to it.
    {
        string interactActionKey;

        if (inputDeviceChanged == false) // If the center screen widget hasn't changed, then continue using the cached string.
        {
            interactActionKey = cachedInteractButtonText;
        }
        else // if statement above prevents below branch from being called in successive loops, to save on performance. Below branch calls a potentially expensive text parsing operation.
        {
            interactActionKey = interactButtonIconReplacer.SetConvertedText_SlowPerformance(); // Try to avoid calling this in a loop
            cachedInteractButtonText = interactActionKey;
            inputDeviceChanged = false; // Set to false after applying changes to prevent future loops from running back into here.
        }

        string relevantInteractAction = baseString;

        string fullText = $"{relevantInteractAction}: {interactActionKey}"; // Display the action key

        return fullText;
    }

    private void DoModeSwitchCleanup()
    {
        tooltipUiComponent.text = "";
    }

    // Getter Setter

    public bool GetSecondTooltipActive()
    {
        return secondTooltipActive;
    }

    public void SetSecondTooltipActive(bool isActive)
    {
        secondTooltipActive = isActive;
    }


}
