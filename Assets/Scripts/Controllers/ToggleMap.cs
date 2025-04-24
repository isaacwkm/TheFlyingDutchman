using System;
using Needle.Console;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleMap : MonoBehaviour
{
    public Camera mainCam;
    public Transform mapVantagePosition;
    private InputSystem_Actions inputActions;
    private int currentCamera = 0;
    private enum Cameras
    {
        Player,
        Map
    }

    private Vector3 cameraOriginalPosition;
    private Quaternion cameraCachedRotation;

    void Awake()
    {
        cameraOriginalPosition = mainCam.transform.localPosition;
        inputActions = InputModeManager.Instance.inputActions;
    }

    void OnEnable()
    {
        inputActions.Player.OpenMap.performed += ctx => SwitchToCamera(1);
        inputActions.UI.CloseMap.performed += ctx => SwitchToCamera(0);
    }

    void OnDisable()
    {
        inputActions.Player.OpenMap.performed -= ctx => SwitchToCamera(1);
        inputActions.UI.CloseMap.performed -= ctx => SwitchToCamera(0);
    }

    public void Toggle()
    {
        Debug.Log("Toggle Pressed! Current Camera: " + currentCamera); // <-- Add this
        
        if (currentCamera == (int)Cameras.Player)
        {
            SwitchToCamera((int)Cameras.Map);
        }
        else
        {
            SwitchToCamera((int)Cameras.Player);
        }
    }
    public void SwitchToCamera(int camNumber)
    {
        if (camNumber == (int)Cameras.Map)
        {
            currentCamera = camNumber;
            SaveCameraRotation();
            SceneCore.cinematics.SetHUDActive(false);
            InputModeManager.Instance.SwitchToUIControls();

            mainCam.transform.position = mapVantagePosition.position;
            mainCam.transform.rotation = mapVantagePosition.rotation;

        }
        else if (camNumber == (int)Cameras.Player)
        {
            currentCamera = camNumber;
            SceneCore.cinematics.SetHUDActive(true);
            InputModeManager.Instance.SwitchToPlayerControls();

            mainCam.transform.localPosition = cameraOriginalPosition;
            mainCam.transform.localRotation = cameraCachedRotation;
        }
        else
        {
            D.Log("couldnt find cam", this, "Any");
        }

    }

    private void SaveCameraRotation()
    {
        cameraCachedRotation = mainCam.transform.localRotation;
    }
}