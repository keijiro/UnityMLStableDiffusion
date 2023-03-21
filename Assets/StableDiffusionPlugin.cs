using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;
using Microsoft.Win32.SafeHandles;

namespace StableDiffusion {

public class Pipeline : SafeHandleZeroOrMinusOneIsInvalid
{
    #region SafeHandle implementation

    Pipeline() : base(true) {}

    protected override bool ReleaseHandle()
    {
        _Destroy(handle);
        return true;
    }

    #endregion

    #region Public methods

    public static Pipeline Create(string resourcePath)
      => _Create(resourcePath);

    public void SetConfig(string prompt, int steps, int seed, float guidance)
      => _SetConfig(this, prompt, steps, seed, guidance);

    public void RunGenerator()
      => _Generate(this);

    public IntPtr ImageBufferPointer
      => _GetImage(this);

    #endregion

    #region Unmanaged interface

    const string DllName = "StableDiffusionPlugin";

    [DllImport(DllName, EntryPoint = "SDCreate")]
    static extern Pipeline _Create(string resourcePath);

    [DllImport(DllName, EntryPoint = "SDSetConfig")]
    static extern void _SetConfig
      (Pipeline self, string prompt, int steps, int seed, float guidance);

    [DllImport(DllName, EntryPoint = "SDGenerate")]
    static extern void _Generate(Pipeline self);

    [DllImport(DllName, EntryPoint = "SDGetImage")]
    static extern IntPtr _GetImage(Pipeline self);

    [DllImport(DllName, EntryPoint = "SDDestroy")]
    static extern void _Destroy(IntPtr self);

    #endregion
}

} // namespace StableDiffusion
