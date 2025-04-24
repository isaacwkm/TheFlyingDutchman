using UnityEngine;

public class OnGameStart : MonoBehaviour
{
    public Transform refPoint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // InputModeManager.Instance.EnablePlayerControls(false);

        Cinematics cinematics = SceneCore.cinematics;
        cinematics.SetHUDActive(false);
        InputModeManager.Instance.SwitchToUIControls(); // Switch to Player Controls when the game starts
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
