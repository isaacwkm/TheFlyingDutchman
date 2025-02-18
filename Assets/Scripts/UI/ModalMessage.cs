using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class ModalMessage : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI textObject;

    private InputModeManager inputMan;
    private InputSystem_Actions inputActions;
    private Action<InputAction.CallbackContext> makeClosableAction;
    private Action<InputAction.CallbackContext> closeAction;
    private bool closable = false;
    private bool dismissed = false;
    public event Action OnClose;

    public string text
    {
        get { return textObject.text; }
        set { textObject.text = value; }
    }

    void Awake()
    {
        inputMan = InputModeManager.Instance;
        inputActions = InputModeManager.Instance.inputActions;
    }

    void OnEnable()
    {
        makeClosableAction = ctx => HandleMakeClosableAction();
        closeAction = ctx => HandleCloseAction();
        inputActions.UI.Submit.performed += makeClosableAction;
        inputActions.UI.Cancel.performed += makeClosableAction;
        inputActions.UI.Submit.canceled += closeAction;
        inputActions.UI.Cancel.canceled += closeAction;
    }

    void OnDisable()
    {
        inputActions.UI.Submit.performed -= makeClosableAction;
        inputActions.UI.Cancel.performed -= makeClosableAction;
        inputActions.UI.Submit.canceled -= closeAction;
        inputActions.UI.Cancel.canceled -= closeAction;
    }

    void Start()
    {
        transform.SetParent(SceneCore.canvas.transform, false);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        inputMan.SwitchToUIControls();
    }

    void HandleMakeClosableAction()
    {
        /* Having this extra action enforces that the button press used to dismiss the message
         * is one which initially began while the message was up,
         * rather than a holdover from beforehand
         * (such as the button press used to bring the message up in the first place). */
        closable = true;
    }

    void HandleCloseAction()
    {
        if (closable && !dismissed)
        {
            dismissed = true;
            OnClose?.Invoke();
            inputMan.SwitchToPlayerControls();
            Destroy(gameObject);
        }
    }
}
