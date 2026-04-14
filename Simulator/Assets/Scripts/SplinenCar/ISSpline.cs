using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines; // SplineContainer'a eriþim için eklendi.

public class ISSpline : MonoBehaviour
{
    [field: SerializeField] public string SplineID { get; set; }
    [field: SerializeField] public ISPoint StartIntersection { get; set; }
    [field: SerializeField] public ISPoint EndIntersection { get; set; }
    public float Length { get; private set; }


#if UNITY_EDITOR
    // Bu kýsým sadece Unity Editor'de çalýþýr, oyunun kendisinde yer almaz.
    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(SplineID) && gameObject.name != SplineID)
        {
            gameObject.name = SplineID;
        }
    }
#endif

    /// <summary>
    /// Bu component uyandýðýnda, baðlý olduðu Spline'ýn uzunluðunu hesaplar.
    /// Bu, oyun baþlamadan önce tüm spline uzunluklarýnýn hazýr olmasýný saðlar.
    /// </summary>
    private void Awake()
    {
        // Bu objeye baðlý SplineContainer component'ini bul.
        if (TryGetComponent<SplineContainer>(out var splineContainer))
        {
            this.Length = splineContainer.Spline.GetLength();
        }
        else
        {
            Debug.LogError($"'{gameObject.name}' isimli ISSpline objesinde SplineContainer component'i bulunamadý! Uzunluk hesaplanamadý.", this.gameObject);
        }
    }
}