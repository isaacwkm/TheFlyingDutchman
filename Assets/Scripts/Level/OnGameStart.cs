using DistantLands.Cozy;
using UnityEngine;

public class OnGameStart : MonoBehaviour
{
    public Transform refPoint;
    public GameObject CreditUI;
    public GameObject MapToolTip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cinematics cinematics = SceneCore.cinematics;
        cinematics.SetHUDActive(false);
        CreditUI.SetActive(false); // Activate the credit UI
        InputModeManager.Instance.SwitchToUIControls(); // Switch to UI Controls when the game starts
        MapToolTip.SetActive(false); // Deactivate the map tooltip
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
