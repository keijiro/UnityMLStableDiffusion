using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;
using Microsoft.Win32.SafeHandles;
using System;

namespace StableDiffusion {

public class Plugin : SafeHandleZeroOrMinusOneIsInvalid
{
    #region SafeHandle implementation

    Plugin() : base(true) {}

    protected override bool ReleaseHandle()
    {
        _Destroy(handle);
        return true;
    }

    #endregion

    #region Public methods

    public static Plugin Create(string resourcePath)
      => _Create(resourcePath);

    public void SetConfig(string prompt, int steps, int seed, float guidance)
      => _SetConfig(this, prompt, steps, seed, guidance);

    public void RunGenerator()
      => _Generate(this);

    public unsafe void RunGeneratorFromImage
      (ReadOnlySpan<byte> image, float strength)
    {
        fixed (byte* ptr = image)
          _GenerateFromImage(this, (IntPtr)ptr, strength);
    }

    public IntPtr ImageBufferPointer
      => _GetImage(this);

    #endregion

    #region Unmanaged interface

#if UNITY_IOS && !UNITY_EDITOR
    const string DllName = "__Internal";
#else
    const string DllName = "StableDiffusionPlugin";
#endif

    [DllImport(DllName, EntryPoint = "SDCreate")]
    static extern Plugin _Create(string resourcePath);

    [DllImport(DllName, EntryPoint = "SDSetConfig")]
    static extern void _SetConfig
      (Plugin self, string prompt, int steps, int seed, float guidance);

    [DllImport(DllName, EntryPoint = "SDGenerate")]
    static extern void _Generate(Plugin self);

    [DllImport(DllName, EntryPoint = "SDGenerateFromImage")]
    static extern void _GenerateFromImage
      (Plugin self, IntPtr image, float strength);

    [DllImport(DllName, EntryPoint = "SDGetImage")]
    static extern IntPtr _GetImage(Plugin self);

    [DllImport(DllName, EntryPoint = "SDDestroy")]
    static extern void _Destroy(IntPtr self);

    #endregion
}

} // namespace StableDiffusion
