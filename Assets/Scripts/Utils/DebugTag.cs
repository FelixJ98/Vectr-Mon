using UnityEngine;

/// <summary>
/// Static class providing centralized debug logging tags for the Vectr-Mon project.
/// Use this class to maintain consistent debug log formatting across all scripts.
/// </summary>
public static class DebugTag
{
    /// <summary>
    /// The main project tag prefix for all debug logs.
    /// </summary>
    public const string VECTR_MON = "[Vectr-Mon]";
    
    /// <summary>
    /// Formats a debug log message with the project tag and class name.
    /// </summary>
    /// <param name="className">The name of the class (use nameof(ClassName) for type safety)</param>
    /// <param name="message">The debug message</param>
    /// <returns>Formatted debug message with tags</returns>
    public static string Format(string className, string message)
    {
        return $"{VECTR_MON} [{className}] {message}";
    }
    
    /// <summary>
    /// Logs a debug message with project and class tags.
    /// </summary>
    /// <param name="className">The name of the class (use nameof(ClassName) for type safety)</param>
    /// <param name="message">The debug message</param>
    public static void Log(string className, string message)
    {
        Debug.Log(Format(className, message));
    }
    
    /// <summary>
    /// Logs a warning message with project and class tags.
    /// </summary>
    /// <param name="className">The name of the class (use nameof(ClassName) for type safety)</param>
    /// <param name="message">The debug message</param>
    public static void LogWarning(string className, string message)
    {
        Debug.LogWarning(Format(className, message));
    }
    
    /// <summary>
    /// Logs an error message with project and class tags.
    /// </summary>
    /// <param name="className">The name of the class (use nameof(ClassName) for type safety)</param>
    /// <param name="message">The debug message</param>
    public static void LogError(string className, string message)
    {
        Debug.LogError(Format(className, message));
    }
}

