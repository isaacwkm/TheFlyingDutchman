using UnityEngine;

public class OnGameStart : MonoBehaviour
{

    public InputModeManager inputSwitcher; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputSwitcher.SwitchToUIControls();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
