// Author: Needle-tools, expanded upon by Isaac Kim
// https://github.com/needle-tools/needle-console/blob/ac8bb15f84c8df319422f2274fd84cc2255b16a7/package/Runtime/Extensions/DebugEditor.cs#L11
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Needle.Console
{
    /// <summary>
    /// Class to log only in Unity editor, double clicking console logs produced by this class still open the calling source file)
    /// NOTE: Implement your own version of this is supported. Just implement a class named "DebugEditor" and any method inside this class starting with "Log" will, when double clicked, open the file of the calling method. Use [Conditional] attributes to control when any of these methods should be included.
    /// </summary>
    public static class D
    {
        [Conditional("UNITY_EDITOR")]
        public static void Log(object message, Object context = null, string category = "Any")
        {
            LogManager.validateLog(category);
            if (LogManager.ShouldLog(category))
                Debug.Log($"[{category}] {message}", context);
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message, Object context = null, string category = "Any")
        {
            LogManager.validateLog(category);
            if (LogManager.ShouldLog(category))
                Debug.LogWarning($"[{category}] {message}", context);
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogError(object message, Object context = null, string category = "Any")
        {
            LogManager.validateLog(category);
            if (LogManager.ShouldLog(category))
                Debug.LogError($"[{category}] {message}", context);
        }
    }
}