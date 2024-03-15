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

    [field:SerializeField][CreateProperty]
    public string Prompt { get; set; } = "dog";

    [field:SerializeField][CreateProperty]
    public float Strength { get; set; } = 0.5f;

    [field:SerializeField][CreateProperty]
    public int StepCount { get; set; } = 10;

    [field:SerializeField][CreateProperty]
    public int Seed { get; set; } = 1234;

    [field:SerializeField][CreateProperty]
    public float GuidanceScale { get; set; } = 10;

    [CreateProperty]
    public string LogText { get; set; }

    #endregion

    #region Editor-only properties

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

    VisualElement UIRoot => GetComponent<UIDocument>().rootVisualElement;
    VisualElement UIPreview => UIRoot.Q("preview");
    VisualElement UIGenerateButton => UIRoot.Q("generate-button");

    async void OnClickGenerate(ClickEvent e) => await RunPipelineAsync();

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
        UIGenerateButton.SetEnabled(false);

        _pipeline = new MLStableDiffusion.Pipeline(_preprocessShader);
        await _pipeline.InitializeAsync(ResourceInfo, _computeUnits);

        LogText = "";
        UIGenerateButton.SetEnabled(true);
    }

    async Awaitable RunPipelineAsync()
    {
        LogText = "Generating...";;
        UIGenerateButton.SetEnabled(false);

        _pipeline.Prompt = Prompt;
        _pipeline.Strength = Strength;
        _pipeline.Scheduler = _scheduler;
        _pipeline.StepCount = StepCount;
        _pipeline.Seed = Seed;
        _pipeline.GuidanceScale = GuidanceScale;

        var time = new Stopwatch();
        time.Start();
        await _pipeline.RunAsync(null, _generated.rt, destroyCancellationToken);
        time.Stop();

        Graphics.CopyTexture(_generated.rt, _generated.tex2d);

        LogText = $"Generation time: {time.Elapsed.TotalSeconds:f2} sec";
        UIGenerateButton.SetEnabled(true);
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        UIRoot.dataSource = this;
        UIGenerateButton.RegisterCallback<ClickEvent>(OnClickGenerate);

        var (w, h) = (_modelSize.x, _modelSize.y);
        _generated.rt = new RenderTexture(w, h, 0, RenderTextureFormat.BGRA32);
        _generated.tex2d = new Texture2D(w, h, TextureFormat.BGRA32, false);
        UIPreview.style.backgroundImage = new StyleBackground(_generated.tex2d);

        _task = SetUpPipelineAsync();
    }

    #endregion
}
