using System;
using System.Collections.Generic;

public class StoryData
{
    private Dictionary<string, object> data = new Dictionary<string, object>();

    /// <summary>
    /// Sets a key-value pair in the dictionary with a generic type.
    /// </summary>
    public void Set<T>(string key, T value)
    {
        data[key] = value;  // Store the value as an object
    }

    /// <summary>
    /// Attempts to retrieve a value from the dictionary safely.
    /// </summary>
    public bool TryGet<T>(string key, out T value)
    {
        if (data.TryGetValue(key, out object obj) && obj is T typedValue)
        {
            value = typedValue;  // Safe casting
            return true;         // Success
        }

        value = default;  // Set default value if not found or incorrect type
        return false;     // Failure
    }
}
