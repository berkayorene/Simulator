using UnityEngine;
using UnityEngine.Splines;
using UnityEditor;

public class IntersectionSetupTool
{
    // YÖNTEM 1: MANUEL KURULUM (DEÐÝÞÝKLÝK YOK)
    [MenuItem("Araçlar/Yol Sistemini Kur/1. Manuel Kurulum (Listelere Göre)")]
    private static void SetupConnectionsManually()
    {
        const string pointTag = "ISPointTag";
        GameObject[] allPointGameObjects = GameObject.FindGameObjectsWithTag(pointTag);

        if (allPointGameObjects.Length == 0)
        {
            Debug.LogWarning($"Sahnede '{pointTag}' etiketine sahip hiç nesne bulunamadý.");
            return;
        }

        foreach (GameObject pointGO in allPointGameObjects)
        {
            ISPoint point = pointGO.GetComponent<ISPoint>();
            if (point == null) continue;

            if (point.OutgoingSplines != null)
            {
                foreach (GameObject splineGO in point.OutgoingSplines)
                {
                    if (splineGO != null && splineGO.TryGetComponent<ISSpline>(out var spline))
                    {
                        Undo.RecordObject(spline, "Set Start Intersection");
                        spline.StartIntersection = point;
                        EditorUtility.SetDirty(spline);
                    }
                }
            }

            if (point.IncomingSplines != null)
            {
                foreach (GameObject splineGO in point.IncomingSplines)
                {
                    if (splineGO != null && splineGO.TryGetComponent<ISSpline>(out var spline))
                    {
                        Undo.RecordObject(spline, "Set End Intersection");
                        spline.EndIntersection = point;
                        EditorUtility.SetDirty(spline);
                    }
                }
            }
        }

        UpdateAllSplineIDsAndNames();
        Debug.Log("Manuel Kurulum Tamamlandý: Baðlantýlar listelere göre kuruldu ve ID'ler güncellendi.");
    }

    //================================================================================
    // YÖNTEM 2: OTOMATÝK KURULUM (GÜNCELLENDÝ)
    //================================================================================
    [MenuItem("Araçlar/Yol Sistemini Kur/2. Otomatik Kurulum (Pozisyona Göre)")]
    private static void SetupConnectionsAutomatically()
    {
        // --- ADIM 1: Önceki Baðlantýlarý Temizle ---
        // Her çalýþtýrmada temiz bir baþlangýç yapmak için tüm ISPoint'lerin listelerini temizle.
        ISPoint[] allPoints = Object.FindObjectsByType<ISPoint>(FindObjectsSortMode.None);
        foreach (ISPoint point in allPoints)
        {
            Undo.RecordObject(point, "Clear Spline Lists");
            point.IncomingSplines.Clear();
            point.OutgoingSplines.Clear();
            EditorUtility.SetDirty(point);
        }

        // --- ADIM 2: Spline'larý Gez ve Çift Yönlü Baðlantýlarý Kur ---
        ISSpline[] allSplines = Object.FindObjectsByType<ISSpline>(FindObjectsSortMode.None);
        if (allSplines.Length == 0)
        {
            Debug.LogWarning("Sahnede hiç ISSpline objesi bulunamadý.");
            return;
        }

        foreach (ISSpline spline in allSplines)
        {
            if (!spline.TryGetComponent<SplineContainer>(out var container) || container.Spline == null || container.Spline.Count < 2)
                continue;

            Undo.RecordObject(spline, "Auto-Detect Intersections");

            // Baþlangýç Noktasýný Bul ve Ata
            Vector3 startPosWorld = spline.transform.TransformPoint(container.Spline[0].Position);
            spline.StartIntersection = FindISPointAt(startPosWorld);

            // TERS YÖNLÜ BAÐLANTIYI KUR (OUTGOING)
            if (spline.StartIntersection != null)
            {
                Undo.RecordObject(spline.StartIntersection, "Add to Outgoing Splines");
                spline.StartIntersection.OutgoingSplines.Add(spline.gameObject);
                EditorUtility.SetDirty(spline.StartIntersection);
            }

            // Bitiþ Noktasýný Bul ve Ata
            Vector3 endPosWorld = spline.transform.TransformPoint(container.Spline[container.Spline.Count - 1].Position);
            spline.EndIntersection = FindISPointAt(endPosWorld);

            // TERS YÖNLÜ BAÐLANTIYI KUR (INCOMING)
            if (spline.EndIntersection != null)
            {
                Undo.RecordObject(spline.EndIntersection, "Add to Incoming Splines");
                spline.EndIntersection.IncomingSplines.Add(spline.gameObject);
                EditorUtility.SetDirty(spline.EndIntersection);
            }

            EditorUtility.SetDirty(spline);
        }

        // --- ADIM 3: ID ve Ýsimleri Güncelle ---
        UpdateAllSplineIDsAndNames();
        Debug.Log("Otomatik Kurulum Tamamlandý: Çift yönlü baðlantýlar pozisyona göre kuruldu ve ID'ler güncellendi.");
    }

    //================================================================================
    // YARDIMCI FONKSÝYONLAR (DEÐÝÞÝKLÝK YOK)
    //================================================================================
    private static void UpdateAllSplineIDsAndNames()
    {
        ISSpline[] allSplines = Object.FindObjectsByType<ISSpline>(FindObjectsSortMode.None);
        foreach (ISSpline spline in allSplines)
        {
            if (spline.StartIntersection != null && spline.EndIntersection != null &&
                !string.IsNullOrEmpty(spline.StartIntersection.IntersectionID) &&
                !string.IsNullOrEmpty(spline.EndIntersection.IntersectionID))
            {
                string newSplineID = $"{spline.StartIntersection.IntersectionID}-{spline.EndIntersection.IntersectionID}";
                if (spline.SplineID != newSplineID)
                {
                    Undo.RecordObject(spline, "Update Spline ID");
                    spline.SplineID = newSplineID;
                    EditorUtility.SetDirty(spline);
                }
                if (spline.gameObject.name != newSplineID)
                {
                    Undo.RecordObject(spline.gameObject, "Rename Spline GameObject");
                    spline.gameObject.name = newSplineID;
                    EditorUtility.SetDirty(spline.gameObject);
                }
            }
        }
    }

    private static ISPoint FindISPointAt(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 0.1f);
        foreach (var col in colliders)
        {
            if (col.TryGetComponent<ISPoint>(out var isPoint))
            {
                return isPoint;
            }
        }
        return null;
    }
}