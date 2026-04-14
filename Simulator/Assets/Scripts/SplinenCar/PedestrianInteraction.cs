using UnityEngine;
using TMPro; // TextMeshPro kütüphanesini kullanmak için bu satýr gerekli.
using System.Collections; // Coroutine'ler için bu satýr gerekli.

public class PedestrianInteraction : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Çarpýþma anýnda gösterilecek olan TextMeshPro UI elemaný.")]
    [SerializeField] private TextMeshProUGUI warningText;

    [Header("Ayarlar")]
    [Tooltip("Yazýnýn ekranda kalma süresi (saniye).")]
    [SerializeField] private float displayDuration = 3f;

    private Coroutine activeCoroutine;

    private void Start()
    {
        // Oyun baþýnda yazýnýn görünmez olduðundan emin ol.
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Warning Text (TextMeshProUGUI) atanmamýþ!", this.gameObject);
        }
    }

    /// <summary>
    /// Uyarý yazýsýný belirli bir süreliðine gösterir.
    /// </summary>
    public void ShowWarning()
    {
        // Eðer zaten çalýþan bir gizleme Coroutine'i varsa, onu durdur.
        // Bu, oyuncu kýsa aralýklarla birden fazla yayaya çarparsa yazýnýn aniden kaybolmasýný engeller.
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        // Coroutine'i baþlat ve referansýný sakla.
        activeCoroutine = StartCoroutine(ShowAndHideRoutine());
    }

    private IEnumerator ShowAndHideRoutine()
    {
        // Yazýyý aktif et.
        warningText.gameObject.SetActive(true);

        // Belirlenen süre kadar bekle.
        yield return new WaitForSeconds(displayDuration);

        // Süre dolduktan sonra yazýyý tekrar pasif et.
        warningText.gameObject.SetActive(false);
    }
}