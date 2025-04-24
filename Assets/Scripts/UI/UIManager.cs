using UnityEngine;

public class UIManager : MonoBehaviour
{
    // Define imported variables here
    public GameObject mainMenuUI;
    public GameObject resourceCountersUI;
    [SerializeField] private GameObject GameplayHUD;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainMenuUI.SetActive(true);
        resourceCountersUI.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        // If something happens
        if (false) // TODO: Change the condition for it make sense
        {
            GameplayHUD.SetActive(true);
        }
    }
}
