using UnityEngine;
using System.Collections;

public class SlidePanel : MonoBehaviour
{
    public RectTransform panelRect;
    public Vector2 slideAmount = new Vector2(300, 0); // Ne kadar kayacak
    public float slideDuration = 0.5f;

    private bool isVisible = false;
    private Coroutine slideCoroutine;

    private Vector2 originalPosition; // Panelin ilk pozisyonu
    private Vector2 targetPosition;   // Kaydıktan sonraki pozisyon

    private void Start()
    {
        originalPosition = panelRect.anchoredPosition;
        targetPosition = originalPosition + slideAmount;
    }

    public void TogglePanel()
    {
        if (isVisible)
            SlideTo(originalPosition);
        else
            SlideTo(targetPosition);

        isVisible = !isVisible;
    }

    public void SlideTo(Vector2 destination)
    {
        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        slideCoroutine = StartCoroutine(Slide(destination));
    }

    private IEnumerator Slide(Vector2 target)
    {
        Vector2 start = panelRect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            panelRect.anchoredPosition = Vector2.Lerp(start, target, elapsed / slideDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        panelRect.anchoredPosition = target;
    }

    public void ResetToOriginal()
    {
        isVisible = false;
        SlideTo(originalPosition);
    }

    public bool IsVisible()
    {
        return isVisible;
    }
}
