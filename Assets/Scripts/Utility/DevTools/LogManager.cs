using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LogManager : MonoBehaviour
{
    // UPDATE TO LOG MANAGER:
    // - Added enum for log categories to introduce type safety and reduce string usage.
    // - Also allows for autocompletion in IDEs when determining valid log categories.
    // - This new enum approach is preferred over the string-based method.
    // - The string-based method is still available for backward compatibility.
    public enum LogCategory
    {
        Any,
        UI,
        Able,
        Aud,
        Inv,
        Item,
        Int,
        Dig,
        Move,
        Story,
        Combat,
        PostProc
    }

    [Header("General Settings")]
    public bool enableLogs = true;
    public bool logAny = true;
    public bool logPostProc = true;

    [Header("UI Logs")]
    public bool logUI = true;
    public bool logAble = true;

    [Header("Audio Logs")]
    public bool logAud = true;

    [Header("Inventory/Item Logs")]
    public bool logInv = true;
    public bool logItem = true;

    [Header("Interaction Logs")]
    public bool logInt = true;
    public bool logDig = true;

    [Header("Player Movement Logs")]
    public bool logMove = true;

    [Header("Story Manager Logs")]
    public bool logStory = true;

    [Header("Combat Logs")]
    public bool logCombat = true;

    private static LogManager instance;

    private Dictionary<LogCategory, bool> categoryStates = new Dictionary<LogCategory, bool>();

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

    private void OnValidate()
    {
        UpdateLogFilters();
    }

    private void UpdateLogFilters()
    {
        categoryStates[LogCategory.Any] = logAny;
        categoryStates[LogCategory.UI] = logUI;
        categoryStates[LogCategory.Able] = logAble;
        categoryStates[LogCategory.Aud] = logAud;
        categoryStates[LogCategory.Inv] = logInv;
        categoryStates[LogCategory.Item] = logItem;
        categoryStates[LogCategory.Int] = logInt;
        categoryStates[LogCategory.Dig] = logDig;
        categoryStates[LogCategory.Move] = logMove;
        categoryStates[LogCategory.Story] = logStory;
        categoryStates[LogCategory.Combat] = logCombat;
        categoryStates[LogCategory.PostProc] = logPostProc;
    }

    // Preferred new method using enum
    public static bool ShouldLog(LogCategory category)
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

    // Backward-compatible method that accepts a string
    public static bool ShouldLog(string categoryStr)
    {
        if (!Enum.TryParse(categoryStr, out LogCategory category))
        {
            category = LogCategory.Any;
        }

        return ShouldLog(category);
    }

    public static string validateLog(string category)
    {
        // Try to parse the string into an enum
        if (Enum.TryParse(category, out LogCategory parsedCategory))
        {
            return parsedCategory.ToString(); // Valid, return normalized name (e.g., fixes casing)
        }

#if UNITY_EDITOR
        Debug.LogWarning($"Unknown log category '{category}' — defaulting to 'Any'");
#endif

        return LogCategory.Any.ToString();
    }

    public static LogCategory validateLog(LogCategory category)
    {
        if (Enum.IsDefined(typeof(LogCategory), category))
        {
            return category;
        }

#if UNITY_EDITOR
        Debug.LogWarning($"Invalid enum log category '{category}' — defaulting to 'Any'");
#endif

        return LogCategory.Any;
    }


}
