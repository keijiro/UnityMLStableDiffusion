using Unity.Collections;
using UnityEngine.Rendering;
using UnityEngine;
using CancellationToken = System.Threading.CancellationToken;

namespace MLStableDiffusion {

public sealed class Pipeline : System.IDisposable
{
    #region Public properties

    public int Width { get; private set; }
    public int Height { get; private set; }
    public string Prompt { get; set; }
    public float Strength { get; set; }
    public int StepCount { get; set; }
    public int Seed { get; set; }
    public float GuidanceScale { get; set; }

    #endregion

    #region Private members

    Plugin _plugin;
    ComputeShader _preprocess;
    (GraphicsBuffer reorder, NativeArray<byte> source, Texture2D output) _buffer;

    // Async readback from a GPU backed texture into a NativeArray
    async Awaitable ReadbackSourceAsync(Texture source)
    {
        // Is gamma decompression needed?
        var isLinear = QualitySettings.activeColorSpace == ColorSpace.Linear;

        // Reordering compute shader invocation
        _preprocess.SetTexture(0, "Input", source);
        _preprocess.SetBuffer(0, "Output", _buffer.reorder);
        _preprocess.SetInts("Size", Width, Height);
        _preprocess.SetBool("IsLinear", isLinear);
        _preprocess.Dispatch(0, Width / 4 / 8, Height / 8, 1);

        // Async readback
        await AsyncGPUReadback.
           RequestIntoNativeArrayAsync(ref _buffer.source, _buffer.reorder);
    }

    #endregion

    #region Public members

    public Pipeline(ComputeShader preprocess)
      => _preprocess = preprocess;

    public void Dispose()
    {
        _buffer.reorder?.Release();
        _buffer.reorder = null;

        if (_buffer.source.IsCreated) _buffer.source.Dispose();

        if (_buffer.output != null) Object.Destroy(_buffer.output);
        _buffer.output = null;

        _plugin?.Dispose();
        _plugin = null;
    }

    public async Awaitable InitializeAsync(ResourceInfo res, ComputeUnits units)
    {
        // Model information
        Width = res.ModelWidth;
        Height = res.ModelHeight;

        // Buffer allocation
        _buffer.reorder = new GraphicsBuffer(GraphicsBuffer.Target.Structured,
                                             Width * Height * 3 / 4, 4);
        _buffer.source = new NativeArray<byte>(Width * Height * 3, Allocator.Persistent,
                                               NativeArrayOptions.UninitializedMemory);
        _buffer.output = new Texture2D(Width, Height, TextureFormat.RGB24, false);

        // Pipeline initialization on the background thread
        await Awaitable.BackgroundThreadAsync();
        _plugin = Plugin.Create(res.ModelPath, units);
        await Awaitable.MainThreadAsync();
    }

    public async Awaitable RunAsync
      (Texture source, RenderTexture dest, CancellationToken cancel)
    {
        // Pipeline configuration
        _plugin.SetConfig(Prompt, StepCount, Seed, GuidanceScale);

        // Source texture async readback
        if (source) await ReadbackSourceAsync(source);

        // Run the pipeline on the background thread.
        await Awaitable.BackgroundThreadAsync();

        if (source)
            _plugin.RunGeneratorFromImage(_buffer.source.AsReadOnlySpan(), Width, Height, Strength);
        else
            _plugin.RunGenerator();

        await Awaitable.MainThreadAsync();
        cancel.ThrowIfCancellationRequested();

        // Load into a temporary texture and flip it into the destination.
        _buffer.output.LoadRawTextureData(_plugin.ImageBufferPointer, Width * Height * 3);
        _buffer.output.Apply();
        Graphics.Blit(_buffer.output, dest, new Vector2(1, -1), new Vector2(0, 1));
    }

    #endregion
}

} // namespace MLStableDiffusion
