using UnityEngine;
using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;

sealed class SwiftPluginTest : MonoBehaviour
{
    [SerializeField] string _resourcePath = "";
    [SerializeField] string _prompt = "";

    [DllImport("SwiftPlugin.dll", EntryPoint = "plugin_create")]
    private static extern IntPtr PluginCreate(string resourcePath, string prompt);

    [DllImport("SwiftPlugin.dll", EntryPoint = "plugin_destroy")]
    private static extern void PluginDestroy(IntPtr self);

    void Start()
    {
        var ptr = PluginCreate(_resourcePath, _prompt);
        if (ptr == IntPtr.Zero)
            Debug.Log("Failed to initialize.");
        else
            PluginDestroy(ptr);
    }
}
