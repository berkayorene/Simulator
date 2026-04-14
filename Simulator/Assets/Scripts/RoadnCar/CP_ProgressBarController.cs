using UnityEngine;
using UnityEngine.UI;

public class CP_ProgressBarController : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private CPManager cp_manager;
    void Start()
    {
        if (progressBar == null)
        {
            Debug.LogError("ProgressBar (Slider) atanmamýþ!", this.gameObject);
            return;
        }

        progressBar.maxValue = cp_manager.TotalPathDistance;
        progressBar.value = 0;
    }

    void Update()
    {
        // Progress bar'ýn mevcut deðerini anlýk olarak kat edilen mesafeye eþitle.
        progressBar.value = cp_manager.CurrentProgressDistance;
    }
}