using UnityEngine;
using System.IO;

public class PrefabTo2DTexture : MonoBehaviour
{
    public Camera captureCamera; // Assign a camera in the scene or create one dynamically
    public int textureSize = 512;

    public Texture2D CapturePrefab(GameObject prefab)
    {
        if (prefab == null || captureCamera == null)
        {
            Debug.LogError("Prefab or capture camera is not assigned!");
            return null;
        }
        
        // Create a temporary instance of the prefab
        GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        
        // Store active state of existing objects and disable them
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (var obj in allObjects)
        {
            if (obj != instance && obj.activeInHierarchy)
                obj.SetActive(false);
        }
        
        // Setup RenderTexture
        RenderTexture renderTexture = new RenderTexture(textureSize, textureSize, 16);
        captureCamera.targetTexture = renderTexture;
        captureCamera.backgroundColor = new Color(0, 0, 0, 0); // Transparent background
        captureCamera.clearFlags = CameraClearFlags.SolidColor;
        
        // Capture image
        captureCamera.Render();
        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
        texture.Apply();
        
        // Restore active state of objects
        foreach (var obj in allObjects)
        {
            if (obj != instance)
                obj.SetActive(true);
        }
        
        // Cleanup
        captureCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);
        Destroy(instance);
        
        return texture;
    }
    
    public void SaveTextureAsPNG(Texture2D texture, string fileName)
    {
        byte[] bytes = texture.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, fileName);
        File.WriteAllBytes(path, bytes);
        Debug.Log("Texture saved: " + path);
    }
}
