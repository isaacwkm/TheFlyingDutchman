using System.Collections.Generic;

public static class Metrics
{
    private static Dictionary<string, float> data = new();
    public static Dictionary<string, float>.KeyCollection keys
    {
        get => data.Keys;
    }
    public static float Get(string key)
    {
        if (data.ContainsKey(key)) return data[key];
        else return 0.0f;
    }
    public static void Set(string key, float value)
    {
        data[key] = value;
    }
    public static void Increment(string key)
    {
        Set(key, Get(key) + 1.0f);
    }
    public static void Decrement(string key)
    {
        Set(key, Get(key) - 1.0f);
    }
    public static void EnsureKeyExists(string key)
    {
        Set(key, Get(key));
    }
    public static void Reset()
    {
        data.Clear();
    }
}
