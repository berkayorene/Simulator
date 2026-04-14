using System.Collections.Generic;

// �sim g�ncellendi.
public class RunSummaryData
{
    // Rota Bilgileri
    public List<string> InitialRoute { get; set; }
    public List<string> FinalLiveRoute { get; set; }
    public List<string> NetProgressRoute { get; set; }
    public List<string> TotalTravelLog { get; set; }

    // Mesafe Bilgileri
    public float InitialRouteLength { get; set; }
    public float FinalLiveRouteLength { get; set; }
    public float NetProgressRouteLength { get; set; }
    public float TotalTravelLogLength { get; set; }

    // Zaman Bilgileri
    public float TotalElapsedTime { get; set; }
    public IReadOnlyDictionary<string, float> CheckpointTimes { get; set; }

    // Performans Metrikleri
    public float EfficiencyPercentage { get; set; }
    public bool WasRouteFinished { get; set; }

    // H�z Bilgileri
    public float AverageSpeedKmh { get; set; }
    public Dictionary<string, float> SegmentSpeedsKmh { get; set; }

    // Genel Bilgiler
    public string CompletionTimestamp { get; set; }
}