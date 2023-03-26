using UnityEngine;
using UnityEngine.UI;
using Stopwatch = System.Diagnostics.Stopwatch;

public sealed class StaticImageTest : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] Texture2D _source = null;
    [Space]
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

    StableDiffusion.Pipeline _pipeline;
    RenderTexture _generated;

    #endregion

    #region Async operations

    async Awaitable SetUpPipelineAsync()
    {
        _uiMessage.text =
          "Loading resources...\n(This takes a few minites for the first time.)";
        _uiGenerate.interactable = false;

        _pipeline = new StableDiffusion.Pipeline(_preprocess);
        await _pipeline.InitializeAsync(ResourcePath);

        _uiMessage.text = "";
        _uiGenerate.interactable = true;

        _generated = new RenderTexture(512, 512, 0);
    }

    async Awaitable RunPipelineAsync()
    {
        _uiMessage.text = "Generating...";
        _uiGenerate.interactable = false;

        _pipeline.Prompt = _uiPrompt.text;
        _pipeline.Strength = _uiStrength.value;
        _pipeline.StepCount = (int)_uiStepCount.value;
        _pipeline.Seed = (int)_uiSeed.value;
        _pipeline.GuidanceScale = _uiGuidance.value;

        var time = new Stopwatch();
        time.Start();
        await _pipeline.RunAsync(_source, _generated);
        time.Stop();

        _uiMessage.text = $"Generation time: {time.Elapsed.TotalSeconds:f2} sec";
        _uiPreview.texture = _generated;
        _uiGenerate.interactable = true;
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
