using System.Collections.Generic;
using UnityEngine;

// This script is attached to a game object, so multiple instances are automatically existent! TODO!!!

[ExecuteAlways] // Allows updating logs in Editor mode
public class LogManager : MonoBehaviour
{
    [Header("General Settings")]
    public bool enableLogs = true;
    [Tooltip("Log Uncategorized and General messages. Keep enabled to receive error logs. If the [Any] channel is overflowing with messages, try to categorize the source of what's causing all the messages.")]
    public bool logAny = true;

    [Tooltip("Log dynamic Post-Processing operations")]
    public bool logPostProc = true;

    [Header("UI Logs")]
    public bool logUI = true;
    [Tooltip("Log Accessibility Systems Logs")]
    public bool logAble = true;

    [Header("Audio Logs")]
    [Tooltip("Log Audio")]
    public bool logAud = true;

    [Header("Inventory/Item Logs")]
    [Tooltip("Log Inventory System")]
    public bool logInv = true;
    public bool logItem = true;

    [Header("Interaction Logs")]
    public bool logInt = true;

    [Tooltip("Shovel Digging Logs (Part of Interactions)")]
    public bool logDig = true;

    [Header("Player Movement Logs")]
    public bool logMove = true;

    [Header("Story Manager Logs")]
    public bool logStory = true;

    [Header("Combat Logs")]
    public bool logCombat = true;
    
    private static LogManager instance;

    private Dictionary<string, bool> categoryStates = new Dictionary<string, bool>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Debug.LogWarning("Multiple LogManager instances detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        UpdateLogFilters();
    }

    private void OnValidate() // Automatically updates categories when a checkbox is toggled
    {
        UpdateLogFilters();
    }

    private void UpdateLogFilters()
    {
        categoryStates["Any"] = logAny; // Any = uncategorized. Any can be disabled and still have other categories show up.
        categoryStates["UI"] = logUI;
        categoryStates["Able"] = logAble; // Accessibility systems
        categoryStates["Aud"] = logAud;
        categoryStates["Inv"] = logInv; // inventory system
        categoryStates["Int"] = logInt; // interaction system
        categoryStates["Dig"] = logDig; // Shovel dig feature logs
        categoryStates["Item"] = logItem; // interaction system
        categoryStates["Move"] = logMove; // player character movement and abilities
        categoryStates["Story"] = logStory; // story manager
        categoryStates["Combat"] = logCombat;
        categoryStates["PostProc"] = logPostProc; // Post processing operations (during runtime)
    }

    public static bool ShouldLog(string category)
    {
        if (instance == null)
        {
            Debug.LogWarning("No LogManager instance found in the scene! Defaulting to true.");
            return true;
        }

        if (!instance.enableLogs)
        {
            return false;
        }

        return instance.categoryStates.TryGetValue(category, out bool isEnabled) && isEnabled;
    }

    public static string validateLog(string category) // Sets category to "Any" if it doesn't exist.
    {
        // Check if the category exists
        if (!instance.categoryStates.ContainsKey(category))
        {
            // If the category doesn't exist, add it with the default value 'true'
            return "Any";
        }
        else{
            return category;
        }
    }
}
