using System;
using System.Diagnostics;
using UnityEngine;

namespace AAA.Outline
{
    /// <summary>
    /// Centralized logging utility for consistent, categorized logs across the project.
    /// Use categories to filter and identify modules faster.
    /// </summary>
    public static class UnityGameLogger
    {
        /// <summary>Writes a debug log with category.</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Debug(string category, string message, UnityEngine.Object context = null)
        {
            if (context != null) UnityEngine.Debug.Log($"[{category}] {message}", context);
            else UnityEngine.Debug.Log($"[{category}] {message}");
        }

        /// <summary>Writes a warning log with category.</summary>
        public static void Warn(string category, string message, UnityEngine.Object context = null)
        {
            if (context != null) UnityEngine.Debug.LogWarning($"[{category}] {message}", context);
            else UnityEngine.Debug.LogWarning($"[{category}] {message}");
        }

        /// <summary>Writes an error log with category.</summary>
        public static void Error(string category, string message, UnityEngine.Object context = null)
        {
            if (context != null) UnityEngine.Debug.LogError($"[{category}] {message}", context);
            else UnityEngine.Debug.LogError($"[{category}] {message}");
        }

        /// <summary>Utility to log exceptions with context.</summary>
        public static void Exception(string category, Exception ex, UnityEngine.Object context = null)
        {
            var msg = $"[{category}] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
            if (context != null) UnityEngine.Debug.LogError(msg, context);
            else UnityEngine.Debug.LogError(msg);
        }
    }
}