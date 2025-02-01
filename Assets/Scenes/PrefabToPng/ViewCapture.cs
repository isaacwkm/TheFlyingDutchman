using UnityEngine;
using System.IO;
using System;
using System.Text.RegularExpressions;

public class ViewCapture : MonoBehaviour
{
    public Camera captureCamera; // Assign the camera in the Inspector
    public int captureWidth = 1920; // Width of the captured image
    public int captureHeight = 1080; // Height of the captured image

    public void CaptureView(string customFileName)
    {
        // Ensure the camera is assigned
        if (captureCamera == null)
        {
            Debug.LogError("Capture Camera is not assigned!");
            return;
        }

        // Create a render texture to capture the camera's view
        RenderTexture renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
        captureCamera.targetTexture = renderTexture;

        // Create a texture with an alpha channel
        Texture2D screenshot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGBA32, false);

        // Render the camera's view into the render texture
        captureCamera.Render();
        RenderTexture.active = renderTexture;
        screenshot.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
        screenshot.Apply(); // Apply the pixel changes

        // Clean up
        captureCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        // Generate a timestamp
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // Sanitize the custom file name
        string sanitizedFileName = SanitizeFileName(customFileName);

        // Combine sanitized file name with timestamp
        string fileName = $"{sanitizedFileName}_{timestamp}.png";

        // Get the path to the Documents folder
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        // Save the image to the Documents folder
        string filePath = Path.Combine(documentsPath, fileName);
        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);

        Debug.Log($"View captured and saved to: {filePath}");
    }

    // Method to sanitize file names by removing invalid characters
    private string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return "Capture"; // Default name if no input is provided
        }

        // Define a regex pattern for invalid characters
        string invalidChars = new string(Path.GetInvalidFileNameChars());
        string invalidRegex = $"[{Regex.Escape(invalidChars)}]";

        // Replace invalid characters with an underscore (_)
        string sanitized = Regex.Replace(fileName, invalidRegex, "_");

        // Trim leading/trailing spaces and ensure the name is not empty
        sanitized = sanitized.Trim();
        if (string.IsNullOrEmpty(sanitized))
        {
            sanitized = "Capture"; // Fallback name if the sanitized name is empty
        }

        return sanitized;
    }
}
