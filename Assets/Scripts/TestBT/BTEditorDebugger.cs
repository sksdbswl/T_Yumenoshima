public static class BTEditorDebugger
{
    public static System.Action<string> OnNodeActive;

    /// <summary>
    /// Active Node Highlighting
    /// </summary>
    public static void SetActive(string guid)
    {
        OnNodeActive?.Invoke(guid);
    }
}