using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

/// <summary>
/// RouteManager'daki aktif spline üzerinde oyuncuya en yakýn noktayý hesaplar
/// ve bu bilgiyi RouteManager'a geri bildirir.
/// </summary>
public class ClosestPointOnSpline : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Rota bilgilerini almak için RouteManager referansý.")]
    [SerializeField] private RouteManager routeManager;

    [Tooltip("Konumu takip edilecek oyuncu veya nesne.")]
    [SerializeField] private Transform playerTransform;

    [Header("Hesaplama Ayarlarý")]
    [Tooltip("En yakýn nokta aramasýnýn çözünürlüðü. Yüksek deðerler daha hassastýr.")]
    [Range(1, 30)]
    [SerializeField] private int resolution = 10;

    [Tooltip("Hassasiyeti artýrmak için yapýlan ekstra iterasyon sayýsý.")]
    [Range(1, 5)]
    [SerializeField] private int iterations = 2;

    private void Update()
    {
        // Gerekli referanslar atanmamýþsa veya rota hazýr deðilse iþlem yapma.
        if (routeManager == null || playerTransform == null || !routeManager.RouteInitialized || routeManager.RouteFinished)
        {
            return;
        }

        // RouteManager'dan mevcut aktif ilk spline'ý al.
        ISSpline currentActiveSpline = routeManager.GetCurrentActiveSpline();

        // Aktif bir spline yoksa veya bu spline'a baðlý bir SplineContainer yoksa iþlem yapma.
        if (currentActiveSpline == null)
        {
            return;
        }

        var splineContainer = currentActiveSpline.GetComponent<SplineContainer>();
        if (splineContainer == null || splineContainer.Spline == null)
        {
            return;
        }

        // --- EN YAKIN NOKTA HESAPLAMASI ---

        // 1. Oyuncunun dünya konumunu spline'ýn lokal uzayýna çevir.
        float3 playerLocalPosition = splineContainer.transform.InverseTransformPoint(playerTransform.position);

        // 2. En yakýn noktayý lokal uzayda ve normalize edilmiþ (t) deðerini bul.
        SplineUtility.GetNearestPoint(
            splineContainer.Spline,
            playerLocalPosition,
            out float3 nearestPointLocal, // En yakýn noktanýn lokal konumu
            out float tValue,              // Spline üzerindeki normalize (0-1) konumu
            resolution,
            iterations
        );

        // 3. Bulunan lokal noktayý tekrar dünya uzayýna çevir.
        Vector3 nearestPointWorld = splineContainer.transform.TransformPoint(nearestPointLocal);

        Debug.DrawLine(playerTransform.position, nearestPointWorld, Color.green);

        // 4. Hesaplanan verileri RouteManager'a bildirerek güncellemesini saðla.
        routeManager.UpdatePlayerProgress(nearestPointWorld, tValue, currentActiveSpline);
    }
}