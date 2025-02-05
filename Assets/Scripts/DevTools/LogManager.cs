using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways] // Allows updating logs in Editor mode
public class LogManager : MonoBehaviour
{
    [Header("Log Categories")]
    public bool enableLogs = true;
    public bool logAny = true; // Any = uncategorized. Any can be disabled and still have other categories show up.
    public bool logUI = true;
    public bool logInv = true; // inventory system
    public bool logInt = true; // interaction system

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
        categoryStates["Inv"] = logInv; // inventory system
        categoryStates["Int"] = logInt; // interaction system
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
