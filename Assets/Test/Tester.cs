using UnityEngine;
using UnityEngine.UI;
using Stopwatch = System.Diagnostics.Stopwatch;
using ImageSource = Klak.TestTools.ImageSource;
using ComputeUnits = MLStableDiffusion.ComputeUnits;
using Scheduler = MLStableDiffusion.Scheduler;

public sealed class Tester : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] ImageSource _source = null;
    [Space]
    [SerializeField] string _resourceDir = "StableDiffusion";
    [SerializeField] Vector2Int _modelSize = new Vector2Int(512, 512);
    [SerializeField] ComputeUnits _computeUnits = ComputeUnits.CpuAndNE;
    [SerializeField] Scheduler _scheduler = Scheduler.Dpmpp;
    [Space]
    [SerializeField] InputField _uiPrompt = null;
    [SerializeField] Slider _uiStrength = null;
    [SerializeField] Slider _uiStepCount = null;
    [SerializeField] Slider _uiSeed = null;
    [SerializeField] Slider _uiGuidance = null;
    [SerializeField] Dropdown _uiPrefilter = null;
    [SerializeField] Button _uiGenerate = null;
    [SerializeField] RawImage _uiPreview = null;
    [SerializeField] RawImage _uiResult = null;
    [SerializeField] Text _uiMessage = null;

    #endregion

    #region Project asset references

    [SerializeField, HideInInspector] ComputeShader _preprocessShader = null;
    [SerializeField, HideInInspector] Shader _prefilterShader = null;

    #endregion

    #region Stable Diffusion pipeline objects

    string ResourcePath
      => Application.streamingAssetsPath + "/" + _resourceDir;

    MLStableDiffusion.ResourceInfo ResourceInfo
      => MLStableDiffusion.ResourceInfo.FixedSizeModel
           (ResourcePath, _modelSize.x, _modelSize.y);

    MLStableDiffusion.Pipeline _pipeline;
    (Material material, RenderTexture texture) _prefilter;
    RenderTexture _generated;
    Awaitable _task;

    #endregion

    #region Async operations

    async Awaitable SetUpPipelineAsync()
    {
        _uiMessage.text =
          "Loading resources...\n(This takes a few minites for the first time.)";
        if (_uiGenerate != null) _uiGenerate.interactable = false;

        _pipeline = new MLStableDiffusion.Pipeline(_preprocessShader);
        await _pipeline.InitializeAsync(ResourceInfo, _computeUnits);

        _uiMessage.text = "";
        if (_uiGenerate != null) _uiGenerate.interactable = true;
    }

    async Awaitable RunPipelineAsync()
    {
        if (_uiGenerate != null)
        {
            _uiMessage.text = "Generating...";
            _uiGenerate.interactable = false;
        }

        _pipeline.Prompt = _uiPrompt.text;
        _pipeline.Strength = _uiStrength.value;
        _pipeline.Scheduler = _scheduler;
        _pipeline.StepCount = (int)_uiStepCount.value;
        _pipeline.Seed = (int)_uiSeed.value;
        _pipeline.GuidanceScale = _uiGuidance.value;

        var time = new Stopwatch();
        time.Start();
        await _pipeline.RunAsync(_prefilter.texture, _generated, destroyCancellationToken);
        time.Stop();

        _uiMessage.text = $"Generation time: {time.Elapsed.TotalSeconds:f2} sec";
        if (_uiGenerate != null) _uiGenerate.interactable = true;
    }

    #endregion

    #region UI callback

    public async void OnClickGenerate() => await RunPipelineAsync();

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _prefilter.material = new Material(_prefilterShader);
        _prefilter.texture = new RenderTexture(_modelSize.x, _modelSize.y, 0);
        _generated = new RenderTexture(_modelSize.x, _modelSize.y, 0);

        _uiPreview.texture = _prefilter.texture;
        _uiResult.texture = _generated;

        _task = SetUpPipelineAsync();
    }

    void OnDestroy()
    {
        _pipeline?.Dispose();
        _pipeline = null;

        Destroy(_prefilter.material);
        Destroy(_prefilter.texture);
        _prefilter = (null, null);

        Destroy(_generated);
        _generated = null;
    }

    void Update()
    {
        Graphics.Blit(_source.AsTexture, _prefilter.texture, _prefilter.material, _uiPrefilter.value);
        if (_uiGenerate == null && _task.IsCompleted) _task = RunPipelineAsync();
    }

    #endregion
}
