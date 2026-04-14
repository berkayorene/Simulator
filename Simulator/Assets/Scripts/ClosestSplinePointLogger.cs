using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class ClosestSplinePointLogger : MonoBehaviour
{
    public SplineContainer splineContainer;
    public Transform player;
    public Transform follower; // isteðe baðlý: takip eden obje

    [Range(1, 20)] public int resolution = 10;
    [Range(1, 5)] public int iterations = 2;

    void Update()
    {
        if (splineContainer == null || player == null)
            return;

        var spline = splineContainer.Spline;

        // 1. Player'ýn dünya konumunu spline'ýn lokal uzayýna çevir.
        // DÜZELTME BURADA!
        float3 playerLocalPosition = splineContainer.transform.InverseTransformPoint(player.position);

        // 2. En yakýn noktayý lokal uzayda bul.
        float3 nearestLocalPoint;
        float nearestT;
        SplineUtility.GetNearestPoint(spline, playerLocalPosition, out nearestLocalPoint, out nearestT, resolution, iterations);

        // 3. Bulunan lokal noktayý dünya uzayýna çevir.
        // Önceden gelen 'out' parametresini kullanmak daha verimlidir.
        Vector3 worldPoint = splineContainer.transform.TransformPoint((Vector3)nearestLocalPoint);

        Debug.Log($"[Nearest Point] t: {nearestT:F3} | Pos: {worldPoint}");
        Debug.DrawLine(player.position, worldPoint, Color.cyan);

        if (follower != null)
        {
            follower.position = worldPoint;
        }
    }
}