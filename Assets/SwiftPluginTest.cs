using UnityEngine;
using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;

sealed class SwiftPluginTest : MonoBehaviour
{
    [SerializeField] string _resourcePath = "coreml-stable-diffusion-2-base/split_einsum/compiled";
    [SerializeField] string _prompt = "a photo of a dog";
    [SerializeField] int _stepCount = 25;
    [SerializeField] int _seed = 100;
    [SerializeField] float _guidanceScale = 8;


    [DllImport("SwiftPlugin.dll", EntryPoint = "plugin_create")]
    private static extern IntPtr PluginCreate(string resourcePath);

    [DllImport("SwiftPlugin.dll", EntryPoint = "plugin_set_config")]
    private static extern void PluginSetConfig
      (IntPtr self, string prompt, int steps, int seed, float guidance);

    [DllImport("SwiftPlugin.dll", EntryPoint = "plugin_generate")]
    private static extern void PluginGenerate(IntPtr self);

    [DllImport("SwiftPlugin.dll", EntryPoint = "plugin_get_image")]
    private static extern IntPtr PluginGetImage(IntPtr self);

    [DllImport("SwiftPlugin.dll", EntryPoint = "plugin_destroy")]
    private static extern void PluginDestroy(IntPtr self);

    void Start()
    {
        var ptr = PluginCreate(_resourcePath);
        if (ptr == IntPtr.Zero)
        {
            Debug.Log("Failed to initialize.");
            return;
        }

        PluginSetConfig(ptr, _prompt, _stepCount, _seed, _guidanceScale);
        PluginGenerate(ptr);

        var tex = new Texture2D(512, 512, TextureFormat.RGB24, false);
        tex.LoadRawTextureData(PluginGetImage(ptr), 512 * 512 * 3);
        tex.Apply();

        PluginDestroy(ptr);

        GetComponent<Renderer>().material.mainTexture = tex;
    }
}
