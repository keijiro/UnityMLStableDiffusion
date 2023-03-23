using UnityEngine;
using UnityEngine.UI;

public sealed class ImageGenerator : MonoBehaviour
{
    [SerializeField] string _prompt = "a photo of a dog";
    [SerializeField] int _stepCount = 25;
    [SerializeField] int _seed = 100;
    [SerializeField] float _guidanceScale = 8;
    [SerializeField] RawImage _preview = null;

    StableDiffusion.Pipeline _pipeline;
    Awaitable _task;

    string ResourcePath
      => Application.streamingAssetsPath + "/StableDiffusion";

    async Awaitable RunGeneratorAsync()
    {
        await Awaitable.BackgroundThreadAsync();
        if (_pipeline == null)
            _pipeline = StableDiffusion.Pipeline.Create(ResourcePath);
        _pipeline.SetConfig(_prompt, _stepCount, _seed, _guidanceScale);
        _pipeline.RunGenerator();
    }

    void Start()
      => _task = RunGeneratorAsync();

    void OnDestroy()
    {
        _task?.Cancel();
        _task = null;

        _pipeline?.Dispose();
        _pipeline = null;
    }

    void Update()
    {
        if (_task?.IsCompleted ?? false)
        {
            var tex = new Texture2D(512, 512, TextureFormat.RGB24, false);
            tex.LoadRawTextureData(_pipeline.ImageBufferPointer, 512 * 512 * 3);
            tex.Apply();

            _preview.texture = tex;

            _task = null;
        }
    }
}