public static class BTEditorDebugger
{
    public static System.Action<string> OnNodeActive;

    public static void SetActive(string guid)
    {
        OnNodeActive?.Invoke(guid);
    }
}