using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Bu script, oyun sýrasýnda bir tuþa basarak RouteManager'ýn dinamik rota
/// deðiþtirme özelliðini manuel olarak test etmek için kullanýlýr.
/// </summary>
public class RouteDebugTester : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Komut gönderilecek olan RouteManager.")]
    [SerializeField] private RouteManager routeManager;

    [Header("Test Ayarlarý")]
    [Tooltip("Rota deðiþikliðini tetikleyecek olan klavye tuþu.")]
    [SerializeField] private KeyCode triggerKey = KeyCode.T;

    // YENÝ: Test rotasý artýk Inspector'dan ayarlanabilir.
    [Tooltip("Tetikleyici tuþa basýldýðýnda uygulanacak olan yeni rota.")]
    [SerializeField] private List<string> newTestRoute = new List<string>();


    private void Update()
    {
        // Belirlenen tuþa basýlýp basýlmadýðýný kontrol et.
        if (Input.GetKeyDown(triggerKey))
        {
            ChangeRoute();
        }
    }

    /// <summary>
    /// Inspector'dan tanýmlanmýþ yeni test rotasýný RouteManager'a gönderir.
    /// </summary>
    private void ChangeRoute()
    {
        if (routeManager == null)
        {
            Debug.LogError("RouteManager referansý atanmamýþ! Test yapýlamýyor.", this.gameObject);
            return;
        }

        if (newTestRoute.Count < 2)
        {
            Debug.LogWarning("New Test Route listesi en az 2 eleman içermelidir! Lütfen Inspector'dan ayarlayýn.", this.gameObject);
            return;
        }

        // DEÐÝÞTÝ: Rota tanýmý artýk buradan kaldýrýldý ve Inspector'dan gelen liste kullanýlýyor.

        Debug.LogWarning($"--- TEST BAÞLATILDI ({triggerKey} tuþuna basýldý) ---");
        Debug.Log($"Yeni test rotasý RouteManager'a gönderiliyor: {string.Join("->", newTestRoute)}");

        // RouteManager'daki ana metodu çaðýrarak dinamik güncellemeyi tetikle.
        routeManager.UpdateLiveRoute(newTestRoute);
    }
}
