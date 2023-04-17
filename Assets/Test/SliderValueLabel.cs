using UnityEngine;
using UnityEngine.UI;

public sealed class SliderValueLabel : MonoBehaviour
{
    [SerializeField] bool isInteger = false;

    public void OnValueChanged(float value)
      => GetComponent<Text>().text = isInteger ? $"{(int)value}" : $"{value:f2}";
}
