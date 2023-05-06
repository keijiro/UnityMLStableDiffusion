using UnityEngine;
using UnityEngine.UI;
using ComputeUnits = MLStableDiffusion.ComputeUnits;
using OperationCanceledException = System.OperationCanceledException;

public sealed class Benchmark : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] string _resourceDir = "StableDiffusion";
    [SerializeField] Vector2Int _modelSize = new Vector2Int(512, 512);
    [SerializeField] ComputeUnits _computeUnits = ComputeUnits.CpuAndNE;
    [Space]
    [SerializeField] string _prompt = "a dog";
    [SerializeField] int _stepCount = 4;
    [SerializeField] float _guidance = 10;
    [Space]
    [SerializeField] RawImage _uiPreview = null;

    #endregion

    #region Project asset references

    [SerializeField, HideInInspector] ComputeShader _preprocessShader = null;

    #endregion

    #region Stable Diffusion pipeline objects

    string ResourcePath
      => Application.streamingAssetsPath + "/" + _resourceDir;

    MLStableDiffusion.ResourceInfo ResourceInfo
      => MLStableDiffusion.ResourceInfo.FixedSizeModel
           (ResourcePath, _modelSize.x, _modelSize.y);

    MLStableDiffusion.Pipeline _pipeline;

    #endregion

    #region MonoBehaviour implementation

    async void Start()
    {
        _pipeline = new MLStableDiffusion.Pipeline(_preprocessShader)
         { Prompt = _prompt, StepCount = _stepCount, GuidanceScale = _guidance };

        await _pipeline.InitializeAsync(ResourceInfo, _computeUnits);

        var rt = new RenderTexture(_modelSize.x, _modelSize.y, 0);
        _uiPreview.texture = rt;

        try
        {
            while (true)
            {
                _pipeline.Seed = Random.Range(0, 0xfffffff);
                await _pipeline.RunAsync(null, rt, destroyCancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _pipeline?.Dispose();
            _pipeline = null;
        }
    }

    #endregion
}
