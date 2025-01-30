using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class CaptureButtonHandler : MonoBehaviour
{
    public PrefabTo2DTexture prefabToPNG; // Reference to the PrefabToPNG script
    public GameObject prefabToCapture; // The prefab you want to capture as a PNG

    public Button captureButton; // Reference to the UI button
    public string savePath = "Textures/ItemIcons"; // Path to save PNGs

    void Start()
    {
        // Ensure the button has the method hooked up on click
        captureButton.onClick.AddListener(OnCaptureButtonClick);
    }

    // Method that gets called when the button is clicked
    void OnCaptureButtonClick()
    {
        if (prefabToPNG == null || prefabToCapture == null)
        {
            Debug.LogError("PrefabToPNG or PrefabToCapture is not assigned!");
            return;
        }

        Texture2D capturedTexture = prefabToPNG.CapturePrefab(prefabToCapture);
        if (capturedTexture != null)
        {
            string filePath = Path.Combine(savePath, prefabToCapture.name + ".png");

            // Make sure the save directory exists
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath); // Create directory if it doesn't exist
            }

            // Prevent overwriting by renaming the file if it exists
            filePath = PreventFileOverwrite(filePath);

            try
            {
                prefabToPNG.SaveTextureAsPNG(capturedTexture, filePath);
                Debug.Log("Saved PNG to: " + filePath);
            }
            catch (Exception e)
            {
                Debug.LogError("Error while saving PNG: " + e.Message);
            }
            Debug.Log("Saved PNG to: " + filePath);  // This should run if everything works
        }
        else
        {
            Debug.LogError("Captured texture is null!");
        }
    }


    // Method to prevent overwriting by renaming the file
    string PreventFileOverwrite(string filepath)
    {
        string directory = Path.GetDirectoryName(filepath);
        string extension = Path.GetExtension(filepath);
        string filename = Path.GetFileNameWithoutExtension(filepath);

        // Check if the file already exists
        if (File.Exists(filepath))
        {
            int counter = 1;
            string newFilePath;

            // Loop to find the next available filename (e.g., filepath_1.png, filepath_2.png)
            do
            {
                newFilePath = Path.Combine(directory, $"{filename}_{counter}{extension}");
                counter++;
            } while (File.Exists(newFilePath));  // Continue until no file exists with the new name

            filepath = newFilePath;
        }

        return filepath;
    }
}
