using UnityEngine;
using System.Collections; // Coroutine'ler için gerekli

public class UIManager : MonoBehaviour
{
    [Header("UI Elemanlarý")]
    [Tooltip("Yanlýþ ok toplandýðýnda gösterilecek UI objesi (örn: çarpý iþareti)")]
    [SerializeField] private GameObject crossObject;
    [Tooltip("Doðru ok toplandýðýnda gösterilecek UI objesi (örn: tik iþareti)")]
    [SerializeField] private GameObject checkmarkObject;

    [Header("Gösterim Süresi")]
    [Tooltip("UI elemanlarýnýn ekranda kalacaðý süre (saniye cinsinden)")]

    [SerializeField] private float displayDuration = 1.0f;
    private Coroutine feedbackCoroutine;

    private void Start()
    {
        // Baþlangýçta her iki UI elementini de gizle
        if (crossObject != null)
        {
            crossObject.SetActive(false);
            Debug.Log("baslangýc set aktýf false edýldý croos ýcýn");
        }
        else
        {
            Debug.LogWarning("Cross UI objesi UIManager'a atanmamýþ! Lütfen Inspector'dan atayýn.");
        }

        if (checkmarkObject != null)
        {
            checkmarkObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Checkmark UI objesi UIManager'a atanmamýþ! Lütfen Inspector'dan atayýn.");
        }
    }

    public void ShowCorrectArrowFeedback()
    {
        // Daha güvenli coroutine yönetimi
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        feedbackCoroutine = StartCoroutine(ShowAndHideUI(checkmarkObject));
    }

    public void ShowWrongArrowFeedback()
    {
        // Daha güvenli coroutine yönetimi
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        feedbackCoroutine = StartCoroutine(ShowAndHideUI(crossObject));
    }
    private IEnumerator ShowAndHideUI(GameObject uiObject)
    {
        // Diðerini kapat (ayný anda hem tik hem çarpý olmasýn)
        if (uiObject == checkmarkObject && crossObject.activeSelf) crossObject.SetActive(false);
        if (uiObject == crossObject && checkmarkObject.activeSelf) checkmarkObject.SetActive(false);

        uiObject.SetActive(true);
        yield return new WaitForSeconds(displayDuration);
        uiObject.SetActive(false);
        feedbackCoroutine = null; // Coroutine bitince referansý temizle
    }
}
