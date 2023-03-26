using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Stopwatch = System.Diagnostics.Stopwatch;

public sealed class ImageGenerator : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] Texture2D _source = null;
    [SerializeField] InputField _uiPrompt = null;
    [SerializeField] Slider _uiStrength = null;
    [SerializeField] Slider _uiStepCount = null;
    [SerializeField] Slider _uiSeed = null;
    [SerializeField] Slider _uiGuidance = null;
    [SerializeField] Button _uiGenerate = null;
    [SerializeField] RawImage _uiPreview = null;
    [SerializeField] Text _uiMessage = null;

    #endregion

    #region Project asset references

    [SerializeField, HideInInspector] ComputeShader _preprocess = null;

    #endregion

    #region Stable Diffusion pipeline objects

    string ResourcePath
      => Application.streamingAssetsPath + "/StableDiffusion";

    StableDiffusion.Plugin _pipeline;
    RenderTexture _generated;

    #endregion

    #region Async operations

    // Stable Diffusion pipeline initialization
    async Awaitable SetUpPipelineAsync()
    {
        // UI deactivation
        _uiMessage.text = "Loading model data...";
        _uiGenerate.interactable = false;

        // Pipeline initialization on the background thread
        await Awaitable.BackgroundThreadAsync();
        _pipeline = StableDiffusion.Plugin.Create(ResourcePath);
        await Awaitable.MainThreadAsync();

        // Destination texture
        _generated = new RenderTexture(512, 512, 0);

        // UI reactivation
        _uiMessage.text = "";
        _uiGenerate.interactable = true;
    }

    // Async readback for GPU backed texture
    async Awaitable<NativeArray<byte>> ReadbackSourceAsync()
    {
        // No readback case
        if (_source == null) return default(NativeArray<byte>);

        // Reordering buffer
        var reordered = new GraphicsBuffer(GraphicsBuffer.Target.Structured,
                                           512 * 512 * 3 / 4, 4);

        // Is Gamma decompression needed?
        var isLinear = QualitySettings.activeColorSpace == ColorSpace.Linear;

        // Reordering compute shader invocation
        _preprocess.SetTexture(0, "Input", _source);
        _preprocess.SetBuffer(0, "Output", reordered);
        _preprocess.SetBool("IsLinear", isLinear);
        _preprocess.Dispatch(0, 512 / 4 / 8, 512 / 8, 1);

        // Readback buffer
        var buffer = new NativeArray<byte>(512 * 512 * 3, Allocator.Persistent,
                                           NativeArrayOptions.UninitializedMemory);

        // Async readback
        await AsyncGPUReadback.RequestIntoNativeArrayAsync(ref buffer, reordered);

        reordered.Release();
        return buffer;
    }

    async Awaitable RunPipelineAsync()
    {
        // UI deactivation
        _uiMessage.text = "Generating...";
        _uiGenerate.interactable = false;

        // Configuration from UI
        _pipeline.SetConfig(_uiPrompt.text, (int)_uiStepCount.value,
                            (int)_uiSeed.value, (int)_uiGuidance.value);

        // (I don't want to touch this value on the background thread.)
        var strength = _uiStrength.value;

        // Source texture async readback
        var buffer = await ReadbackSourceAsync();

        // Run the pipeline on the background thread.
        await Awaitable.BackgroundThreadAsync();

        var time = new Stopwatch();
        time.Start();

        if (buffer.IsCreated)
            _pipeline.RunGeneratorFromImage(buffer.AsReadOnlySpan(), strength);
        else
            _pipeline.RunGenerator();

        time.Stop();

        await Awaitable.MainThreadAsync();

        // Source readback buffer deallocation
        if (buffer.IsCreated) buffer.Dispose();

        // Load into a temporary texture and flip it into the destination.
        var tex = new Texture2D(512, 512, TextureFormat.RGB24, false);
        tex.LoadRawTextureData(_pipeline.ImageBufferPointer, 512 * 512 * 3);
        tex.Apply();
        Graphics.Blit(tex, _generated, new Vector2(1, -1), new Vector2(0, 1));
        Destroy(tex);

        // UI reactivation
        _uiMessage.text = $"Generation time: {time.Elapsed.TotalSeconds:f2} sec";
        _uiGenerate.interactable = true;
        _uiPreview.texture = _generated;
    }

    #endregion

    #region UI callback

    public void OnClickGenerate() => RunPipelineAsync();

    #endregion

    #region MonoBehaviour implementation

    void Start() => SetUpPipelineAsync();

    void OnDestroy()
    {
        _pipeline?.Dispose();
        Destroy(_generated);
        (_pipeline, _generated) = (null, null);
    }

    #endregion
}
