using UnityEngine;
using TMPro; // TextMeshPro kullanmak için bu gerekli.
using System;  // TimeSpan kullanmak için bu gerekli.

public class TimerUIController : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Zamaný gösterecek olan TextMeshPro UI elemaný.")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Tooltip("Zaman verisini alacaðýmýz RouteTimerManager.")]
    [SerializeField] private RouteTimerManager routeTimerManager;

    void Start()
    {
        // Gerekli referanslar atanmamýþsa hata ver ve scripti devre dýþý býrak.
        if (timerText == null || routeTimerManager == null)
        {
            Debug.LogError("TimerUIController'da gerekli referanslar atanmamýþ!", this.gameObject);
            enabled = false;
            return;
        }
        // Baþlangýçta metni sýfýrla.
        timerText.text = "00:00:00";
    }

    void Update()
    {
        // Eðer zamanlayýcý çalýþýyorsa metni güncelle.
        if (routeTimerManager.IsRunning)
        {
            // Geçen saniyeyi al.
            float elapsedTime = routeTimerManager.ElapsedTime;

            // Saniyeyi Dakika:Saniye:Milisaniye formatýna çevir.
            TimeSpan time = TimeSpan.FromSeconds(elapsedTime);

            // Metni formatla ve UI elemanýna ata.
            timerText.text = string.Format("{0:00}:{1:00}:{2:00}",
                time.Minutes,
                time.Seconds,
                time.Milliseconds / 10);
        }
    }
}