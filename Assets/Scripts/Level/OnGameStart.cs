using UnityEngine;

public class OnGameStart : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputModeManager.Instance.SwitchToUIControls(); // Switch to Player Controls when the game starts
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
