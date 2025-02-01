using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaptureButtonHandler : MonoBehaviour
{
    public ViewCapture viewCapture; // Assign the ViewCapture script in the Inspector
    public TMP_InputField fileNameInputField; // Assign the InputField in the Inspector

    public void OnCaptureButtonClicked()
    {
        // Get the custom file name from the input field
        string customFileName = fileNameInputField.text;

        // If no custom name is provided, use a default name
        if (string.IsNullOrEmpty(customFileName))
        {
            customFileName = "Capture";
        }

        // Call the CaptureView method with the custom file name
        viewCapture.CaptureView(customFileName);
    }
}