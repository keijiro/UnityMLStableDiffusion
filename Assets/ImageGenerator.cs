using UnityEngine;
using UnityEngine.UI;

public sealed class ImageGenerator : MonoBehaviour
{
    [SerializeField] string _resourcePath = "coreml-stable-diffusion-2-base/split_einsum/compiled";
    [SerializeField] string _prompt = "a photo of a dog";
    [SerializeField] int _stepCount = 25;
    [SerializeField] int _seed = 100;
    [SerializeField] float _guidanceScale = 8;
    [SerializeField] RawImage _preview = null;

    void Start()
    {
        using var sd = StableDiffusion.Pipeline.Create(_resourcePath);

        sd.SetConfig(_prompt, _stepCount, _seed, _guidanceScale);
        sd.RunGenerator();

        var tex = new Texture2D(512, 512, TextureFormat.RGB24, false);
        tex.LoadRawTextureData(sd.ImageBufferPointer, 512 * 512 * 3);
        tex.Apply();

        _preview.texture = tex;
    }
}
