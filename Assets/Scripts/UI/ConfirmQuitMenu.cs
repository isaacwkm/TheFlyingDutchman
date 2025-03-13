using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConfirmQuitMenu : UIStack.Context
{
    [SerializeField] private Button saveAndQuitButton;
    [SerializeField] private Button quitWithoutSavingButton;
    [SerializeField] private Button backButton;

    override protected void OnEnable()
    {
        base.OnEnable();
        EventSystem.current.SetSelectedGameObject(backButton.gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(backButton.gameObject);
        // TODO: implement saveAndQuitButton
        quitWithoutSavingButton.onClick.AddListener(() => Misc.Quit());
        backButton.onClick.AddListener(() => Return());
    }
}
