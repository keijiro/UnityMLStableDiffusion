namespace MLStableDiffusion {

public readonly struct ResourceInfo
{
    public readonly string ModelPath;
    public readonly int ModelWidth;
    public readonly int ModelHeight;

    ResourceInfo(string path, int width, int height)
    {
        ModelPath = path;
        ModelWidth = width;
        ModelHeight = height;
    }

    public static implicit operator ResourceInfo(string path)
      => new ResourceInfo(path, 512, 512);

    public static ResourceInfo FixedSizeModel(string path, int width, int height)
      => new ResourceInfo(path, width, height);
}

} // namespace MLStableDiffusion
