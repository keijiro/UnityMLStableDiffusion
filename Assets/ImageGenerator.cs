using UnityEngine;
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

    #region Stable Diffusion pipeline objects

    string ResourcePath
      => Application.streamingAssetsPath + "/StableDiffusion";

    StableDiffusion.Pipeline _pipeline;

    #endregion

    #region Async operations

    async Awaitable SetUpPipelineAsync()
    {
        _uiMessage.text = "Loading model data...";
        _uiGenerate.interactable = false;

        await Awaitable.BackgroundThreadAsync();

        _pipeline = StableDiffusion.Pipeline.Create(ResourcePath);

        await Awaitable.MainThreadAsync();

        _uiMessage.text = "";
        _uiGenerate.interactable = true;
    }

    async Awaitable RunPipelineAsync()
    {
        var image = _source != null ? _source.GetRawTextureData() : null;
        var strength = _uiStrength.value;

        _pipeline.SetConfig
          (_uiPrompt.text, (int)_uiStepCount.value,
           (int)_uiSeed.value, (int)_uiGuidance.value);

        _uiMessage.text = "Generating...";
        _uiGenerate.interactable = false;

        await Awaitable.BackgroundThreadAsync();

        var time = new Stopwatch();
        time.Start();

        if (image != null)
            _pipeline.RunGeneratorFromImage(image, strength);
        else
            _pipeline.RunGenerator();

        time.Stop();

        await Awaitable.MainThreadAsync();

        var tex = new Texture2D(512, 512, TextureFormat.RGB24, false);
        tex.LoadRawTextureData(_pipeline.ImageBufferPointer, 512 * 512 * 3);
        tex.Apply();

        _uiMessage.text = $"Generation time: {time.Elapsed.TotalSeconds:f2} sec";
        _uiGenerate.interactable = true;
        _uiPreview.texture = tex;
    }

    #endregion

    #region UI callback

    public void OnClickGenerate()
      => RunPipelineAsync();

    #endregion

    #region MonoBehaviour implementation

    void Start()
      => SetUpPipelineAsync();

    void OnDestroy()
      => _pipeline?.Dispose();

    #endregion
}
