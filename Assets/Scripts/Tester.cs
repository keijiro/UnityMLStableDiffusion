using Unity.Properties;
using UnityEngine.UIElements;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;
using ImageSource = Klak.TestTools.ImageSource;
using ComputeUnits = MLStableDiffusion.ComputeUnits;
using Scheduler = MLStableDiffusion.Scheduler;

public sealed class Tester : MonoBehaviour
{
    #region Properties for UI binding

    [CreateProperty]
    public string LogText { get; set; }

    #endregion

    #region Editor-only properties

    [SerializeField]
    GeneratorSettings _settings = null;

    [SerializeField]
    string _resourceDir = "StableDiffusion";

    [SerializeField]
    Vector2Int _modelSize = new Vector2Int(512, 512);

    [SerializeField]
    ComputeUnits _computeUnits = ComputeUnits.CpuAndNE;

    [SerializeField]
    Scheduler _scheduler = Scheduler.Dpmpp;

    #endregion

    #region Project asset references

    [SerializeField, HideInInspector] ComputeShader _preprocessShader = null;

    #endregion

    #region UI helpers

    ImageSource ImageSource => GetComponent<ImageSource>();

    VisualElement UIRoot => GetComponent<UIDocument>().rootVisualElement;
    VisualElement UIControlPanel => UIRoot.Q("control-panel");
    VisualElement UIImageSourceTabs => UIRoot.Q("image-source-tabs");
    VisualElement UIPreview => UIRoot.Q("preview");
    VisualElement UIGenerateByTextButton => UIRoot.Q("text-generate-button");
    VisualElement UIGenerateByImageButton => UIRoot.Q("image-generate-button");
    VisualElement UIGenerateByWebcamButton => UIRoot.Q("webcam-generate-button");
    VisualElement UILogText => UIRoot.Q("log-label");

    async void OnClickGenerateByTextButton(ClickEvent e)
    {
        UIImageSourceTabs.SetEnabled(false);
        await RunPipelineAsync(null);
        UIImageSourceTabs.SetEnabled(true);
    }

    async void OnClickGenerateByImageButton(ClickEvent e)
    {
        UIImageSourceTabs.SetEnabled(false);
        await RunPipelineAsync(ImageSource.AsTexture);
        UIImageSourceTabs.SetEnabled(true);
    }

    async void OnClickGenerateByWebcamButton(ClickEvent e)
    {
        UIImageSourceTabs.SetEnabled(false);
        await RunPipelineAsync(ImageSource.AsTexture);
        UIImageSourceTabs.SetEnabled(true);
    }

    #endregion

    #region Stable Diffusion pipeline objects

    string ResourcePath
      => Application.streamingAssetsPath + "/" + _resourceDir;

    MLStableDiffusion.ResourceInfo ResourceInfo
      => MLStableDiffusion.ResourceInfo.FixedSizeModel
           (ResourcePath, _modelSize.x, _modelSize.y);

    MLStableDiffusion.Pipeline _pipeline;
    (RenderTexture rt, Texture2D tex2d) _generated;
    Awaitable _task;

    #endregion

    #region Async operations

    async Awaitable SetUpPipelineAsync()
    {
        LogText = "Loading resources...\n" +
          "(This takes a few minites for the first time.)";
        UIImageSourceTabs.SetEnabled(false);

        _pipeline = new MLStableDiffusion.Pipeline(_preprocessShader);
        await _pipeline.InitializeAsync(ResourceInfo, _computeUnits);

        LogText = "";
        UIImageSourceTabs.SetEnabled(true);
    }

    async Awaitable RunPipelineAsync(Texture sourceImage)
    {
        LogText = "Generating...";;

        _pipeline.Prompt = _settings.prompt;
        _pipeline.Strength = _settings.strength;
        _pipeline.Scheduler = _scheduler;
        _pipeline.StepCount = _settings.stepCount;
        _pipeline.Seed = _settings.seed;
        _pipeline.GuidanceScale = _settings.guidance;

        var time = new Stopwatch();
        time.Start();
        await _pipeline.RunAsync(sourceImage, _generated.rt, destroyCancellationToken);
        time.Stop();

        Graphics.CopyTexture(_generated.rt, _generated.tex2d);

        LogText = $"Generation time: {time.Elapsed.TotalSeconds:f2} sec";
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        UIControlPanel.dataSource = _settings;
        UILogText.dataSource = this;

        UIGenerateByTextButton.RegisterCallback<ClickEvent>(OnClickGenerateByTextButton);
        UIGenerateByImageButton.RegisterCallback<ClickEvent>(OnClickGenerateByImageButton);
        UIGenerateByWebcamButton.RegisterCallback<ClickEvent>(OnClickGenerateByWebcamButton);

        var (w, h) = (_modelSize.x, _modelSize.y);
        _generated.rt = new RenderTexture(w, h, 0, RenderTextureFormat.BGRA32);
        _generated.tex2d = new Texture2D(w, h, TextureFormat.BGRA32, false);
        UIPreview.style.backgroundImage = new StyleBackground(_generated.tex2d);

        _task = SetUpPipelineAsync();
    }

    #endregion
}
