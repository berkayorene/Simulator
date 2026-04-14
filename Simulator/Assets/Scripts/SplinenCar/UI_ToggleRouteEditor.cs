using UnityEngine;

/// <summary>
/// Belirli bir UI panelini (Route Editor) açýp kapatma iþlevini yönetir.
/// </summary>
public class UI_ToggleRouteEditor : MonoBehaviour
{
    [Header("UI Panelleri")]
    [Tooltip("Açýlýp kapatýlacak olan Route Editor paneli.")]
    [SerializeField] private GameObject routeEditorPanel;

    /// <summary>
    /// Bu metot, bir UI Butonu tarafýndan çaðrýlmak üzere tasarlanmýþtýr.
    /// RouteEditor panelinin görünürlüðünü açar veya kapatýr.
    /// </summary>
    public void ToggleRouteEditor()
    {
        // routeEditorPanel referansýnýn atanýp atanmadýðýný kontrol et.
        if (routeEditorPanel != null)
        {
            // Panelin mevcut aktif durumunu tersine çevir.
            // Yani, panel açýksa kapatýr; kapalýysa açar.
            bool isPanelActive = routeEditorPanel.activeSelf;
            routeEditorPanel.SetActive(!isPanelActive);
        }
        else
        {
            // Eðer panel atanmamýþsa, konsola bir hata mesajý yazdýrarak geliþtiriciyi uyar.
            Debug.LogError("Route Editor Paneli atanmamýþ! Lütfen UI_ToggleRouteEditor script'indeki 'Route Editor Panel' alanýna ilgili GameObject'i sürükleyin.");
        }
    }
}
