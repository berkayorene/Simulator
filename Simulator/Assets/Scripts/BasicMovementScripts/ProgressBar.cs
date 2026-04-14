using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Slider progressSlider;

    public void SetProgress(float value)
    {
        // Value should be between 0 and 1
        progressSlider.value = Mathf.Clamp01(value);
    }
}
