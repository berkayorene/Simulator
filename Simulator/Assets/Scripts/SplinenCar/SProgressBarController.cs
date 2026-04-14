using UnityEngine;
using UnityEngine.UI; // UI elemanlarýna eriþim için bu kütüphane gereklidir.

/// <summary>
/// RouteManager'daki ilerleme verisine göre bir UI Slider'ýný günceller.
/// </summary>
public class SProgressBarController : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Ýlerleme durumunu gösterecek olan UI Slider objesi.")]
    [SerializeField] private Slider progressBar;

    [Tooltip("Rota ve ilerleme verilerini saðlayacak olan RouteManager objesi.")]
    [SerializeField] private RouteManager routeManager;

    void Start()
    {
        // Gerekli atamalarýn yapýlýp yapýlmadýðýný kontrol et.
        if (progressBar == null)
        {
            Debug.LogError("ProgressBar (Slider) atanmamýþ!", this.gameObject);
            enabled = false; // Script'i devre dýþý býrak.
            return;
        }

        if (routeManager == null)
        {
            Debug.LogError("RouteManager atanmamýþ!", this.gameObject);
            enabled = false; // Script'i devre dýþý býrak.
            return;
        }

        // Slider'ý yüzde (0-1) formatýnda çalýþacak þekilde ayarla.
        progressBar.minValue = 0f;
        progressBar.maxValue = 1f;
        progressBar.value = 0f; // Baþlangýçta ilerleme sýfýr.
    }

    void Update()
    {
        // RouteManager hazýr ve rota baþlamýþsa ilerlemeyi güncelle.
        if (routeManager.RouteInitialized)
        {
            // Toplam rota uzunluðunun sýfýr olmadýðýndan emin ol (bölme hatasýný önlemek için).
            if (routeManager.TotalRouteLength > 0)
            {
                // Ýlerlemeyi 0 ile 1 arasýnda bir deðere dönüþtür.
                // (Gidilen Mesafe / Toplam Mesafe)
                float currentProgress = routeManager.TotalDistanceTraveled / routeManager.TotalRouteLength;

                // Slider'ýn deðerini hesaplanan ilerlemeye eþitle.
                progressBar.value = currentProgress;
            }
        }
        else
        {
            // Rota henüz baþlamadýysa veya sýfýrlandýysa, ilerleme çubuðunu sýfýrda tut.
            progressBar.value = 0f;
        }
    }
}