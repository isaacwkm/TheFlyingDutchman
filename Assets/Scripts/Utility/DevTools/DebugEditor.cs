// Author: Needle-tools, expanded upon by Isaac Kim
// https://github.com/needle-tools/needle-console/blob/ac8bb15f84c8df319422f2274fd84cc2255b16a7/package/Runtime/Extensions/DebugEditor.cs#L11
using System.Diagnostics;
using NUnit.Framework;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using UnityEngine;

namespace Needle.Console
{
    public static class D
    {
        // === ORIGINAL STRING-BASED OVERLOADS (Unchanged) ===
        [Conditional("UNITY_EDITOR")]
        public static void Log(object message, Object context = null, string category = "Any")
        {
            var validated = LogManager.validateLog(category);
            if (LogManager.ShouldLog(validated))
                Debug.Log($"[{validated}] {message}", context);
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message, Object context = null, string category = "Any")
        {
            var validated = LogManager.validateLog(category);
            if (LogManager.ShouldLog(validated))
                Debug.LogWarning($"[{validated}] {message}", context);
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogError(object message, Object context = null, string category = "Any")
        {
            var validated = LogManager.validateLog(category);
            if (LogManager.ShouldLog(validated))
                Debug.LogError($"[{validated}] {message}", context);
        }

        // === NEW ENUM-BASED OVERLOADS ===
        [Conditional("UNITY_EDITOR")]
        public static void Log(object message, Object context, LogManager.LogCategory category)
        {
            if (LogManager.ShouldLog(category))
                Debug.Log($"[{category}] {message}", context);
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message, Object context, LogManager.LogCategory category)
        {
            if (LogManager.ShouldLog(category))
                Debug.LogWarning($"[{category}] {message}", context);
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogError(object message, Object context, LogManager.LogCategory category)
        {
            if (LogManager.ShouldLog(category))
                Debug.LogError($"[{category}] {message}", context);
        }

    }
}
