using UnityEngine;

[CreateAssetMenu]
public sealed class GeneratorSettings : ScriptableObject
{
    public string prompt;
    public int stepCount;
    public int seed;
    public float guidance;
    public float strength;
}
