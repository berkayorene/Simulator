using UnityEngine;
using UnityEngine.Splines; // Spline'lara eriþmek için bu satýr gerekli

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EditorNameTag : MonoBehaviour
{
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Varsayýlan pozisyonu objenin kendi pozisyonu olarak ayarla
        Vector3 labelPosition = transform.position;

        // Bu objenin bir SplineContainer component'i olup olmadýðýný kontrol et
        if (TryGetComponent<SplineContainer>(out var splineContainer))
        {
            // Eðer spline verisi geçerliyse (en az bir noktasý varsa)
            if (splineContainer.Spline != null && splineContainer.Spline.Count > 0)
            {
                // Spline'ýn tam ortasýndaki (%50) noktanýn LOKAL pozisyonunu al
                var localCenter = splineContainer.Spline.EvaluatePosition(0.5f);

                // Lokal pozisyonu, objenin dünya pozisyonuna çevir
                labelPosition = transform.TransformPoint(localCenter);
            }
        }

        // Etiketi, hesaplanan pozisyonda çiz
        Handles.Label(labelPosition, gameObject.name);
    }
#endif
}