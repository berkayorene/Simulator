using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Ýsim güncellendi.
public static class RunSummaryGenerator
{
    // Metotun dönüþ tipi de güncellendi.
    public static RunSummaryData Generate(RouteManager routeManager, RouteTimerManager timerManager)
    {
        // Nesne oluþturma da yeni isimle yapýlýyor.
        var summary = new RunSummaryData();

        // --- TEMEL VERÝLERÝ DOLDUR ---
        summary.InitialRoute = new List<string>(routeManager.InitialRoute);
        summary.FinalLiveRoute = new List<string>(routeManager.LiveRoute);
        summary.NetProgressRoute = new List<string>(routeManager.NetProgressRoute);
        summary.TotalTravelLog = new List<string>(routeManager.TotalTravelLogRoute);
        summary.WasRouteFinished = routeManager.RouteFinished;
        summary.CompletionTimestamp = System.DateTime.Now.ToString("g");

        // --- UZUNLUKLARI HESAPLA ---
        summary.InitialRouteLength = CalculateRouteLength(summary.InitialRoute, routeManager);
        summary.FinalLiveRouteLength = routeManager.TotalRouteLength;
        summary.NetProgressRouteLength = CalculateRouteLength(summary.NetProgressRoute, routeManager);
        summary.TotalTravelLogLength = CalculateRouteLength(summary.TotalTravelLog, routeManager);

        // --- ZAMAN VERÝLERÝNÝ DOLDUR ---
        if (timerManager != null)
        {
            summary.TotalElapsedTime = timerManager.ElapsedTime;
            summary.CheckpointTimes = timerManager.CheckpointTimes;
        }

        // --- PERFORMANS VE HIZ HESAPLAMALARI ---
        // Verimlilik
        if (summary.TotalTravelLogLength > 0)
            summary.EfficiencyPercentage = (summary.NetProgressRouteLength / summary.TotalTravelLogLength) * 100f;
        else
            summary.EfficiencyPercentage = 100f;

        // Ortalama Hýz (km/s)
        if (summary.TotalElapsedTime > 0)
        {
            float averageSpeedMs = summary.TotalTravelLogLength / summary.TotalElapsedTime;
            summary.AverageSpeedKmh = averageSpeedMs * 3.6f;
        }

        // Segment Hýzlarý (km/s)
        summary.SegmentSpeedsKmh = new Dictionary<string, float>();
        if (summary.CheckpointTimes != null && summary.CheckpointTimes.Count > 1)
        {
            for (int i = 0; i < summary.TotalTravelLog.Count - 1; i++)
            {
                string startPointID = summary.TotalTravelLog[i];
                string endPointID = summary.TotalTravelLog[i + 1];

                if (summary.CheckpointTimes.TryGetValue(startPointID, out float startTime) &&
                    summary.CheckpointTimes.TryGetValue(endPointID, out float endTime))
                {
                    if (endTime > startTime)
                    {
                        float segmentTime = endTime - startTime;
                        float segmentDistance = CalculateSegmentLength(startPointID, endPointID, routeManager);

                        if (segmentTime > 0 && segmentDistance > 0)
                        {
                            float segmentSpeedMs = segmentDistance / segmentTime;
                            float segmentSpeedKmh = segmentSpeedMs * 3.6f;
                            string segmentKey = $"{startPointID} -> {endPointID}";
                            summary.SegmentSpeedsKmh[segmentKey] = segmentSpeedKmh;
                        }
                    }
                }
            }
        }

        Debug.Log("Koþu özeti oluþturuldu. Ortalama Hýz: " + summary.AverageSpeedKmh.ToString("F1") + " km/s");
        return summary;
    }

    // Yardýmcý metotlar ayný kalýyor...
    private static float CalculateRouteLength(List<string> routePointIDs, RouteManager routeManager)
    {
        if (routePointIDs == null || routePointIDs.Count < 2) return 0f;
        float totalLength = 0f;
        for (int i = 0; i < routePointIDs.Count - 1; i++)
        {
            totalLength += CalculateSegmentLength(routePointIDs[i], routePointIDs[i + 1], routeManager);
        }
        return totalLength;
    }

    private static float CalculateSegmentLength(string startPointID, string endPointID, RouteManager routeManager)
    {
        var allPointsInScene = Object.FindObjectsByType<ISPoint>(FindObjectsSortMode.None).ToDictionary(p => p.IntersectionID);
        if (allPointsInScene.TryGetValue(startPointID, out var startPoint) && allPointsInScene.TryGetValue(endPointID, out var endPoint))
        {
            foreach (var splineGO in startPoint.OutgoingSplines.Concat(startPoint.IncomingSplines))
            {
                if (splineGO.TryGetComponent<ISSpline>(out var spline))
                {
                    if ((spline.StartIntersection == startPoint && spline.EndIntersection == endPoint) ||
                       (spline.StartIntersection == endPoint && spline.EndIntersection == startPoint))
                    {
                        return spline.Length;
                    }
                }
            }
        }
        return 0f;
    }
}