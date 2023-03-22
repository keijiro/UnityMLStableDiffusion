using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public sealed class ImageGenerator : MonoBehaviour
{
    [SerializeField] string _prompt = "a photo of a dog";
    [SerializeField] int _stepCount = 25;
    [SerializeField] int _seed = 100;
    [SerializeField] float _guidanceScale = 8;
    [SerializeField] RawImage _preview = null;

    StableDiffusion.Pipeline _pipeline;
    Task _task;

    string ResourcePath
      => Application.streamingAssetsPath + "/StableDiffusion";

    void Start()
    {
        _task = Task.Run(() => {
            if (_pipeline == null)
                _pipeline = StableDiffusion.Pipeline.Create(ResourcePath);
            _pipeline.SetConfig(_prompt, _stepCount, _seed, _guidanceScale);
            _pipeline.RunGenerator();
        });
    }

    void OnDestroy()
    {
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

            _task.Dispose();
            _task = null;
        }
    }
}
