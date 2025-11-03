#if UNITY_EDITOR
using System.Reflection;
using UnityEngine;

namespace REIW
{
    public static class EditorUtilities
    {
        public static void ClearEditorConsole()
        {
            var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }
    }
}
#endif
