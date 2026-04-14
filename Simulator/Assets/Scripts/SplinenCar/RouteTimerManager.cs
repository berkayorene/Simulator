using System.Collections.Generic;
using UnityEngine;

public class RouteTimerManager : MonoBehaviour
{
    [Header("Timer Status")]
    [Tooltip("Zamanlayýcýnýn o anki deðeri (saniye).")]
    [field: SerializeField] public float ElapsedTime { get; private set; } = 0f;

    [Tooltip("Zamanlayýcýnýn çalýþýp çalýþmadýðýný gösterir.")]
    [field: SerializeField] public bool IsRunning { get; private set; } = false;

    [Header("Timing Results")]
    [Tooltip("Her bir checkpoint'e ulaþýldýðý zamanýn kaydý.")]
    // Dýþarýdan okunabilmesi için public bir property olarak açýyoruz.
    public IReadOnlyDictionary<string, float> CheckpointTimes => checkpointTimes;
    private Dictionary<string, float> checkpointTimes = new Dictionary<string, float>();

    private void Update()
    {
        // Zamanlayýcý çalýþýyorsa, geçen süreyi artýr.
        if (IsRunning)
        {
            ElapsedTime += Time.deltaTime;
        }
    }

    /// <summary>
    /// Zamanlayýcýný baþlatýr.
    /// </summary>
    public void StartTimer()
    {
        if (!IsRunning)
        {
            IsRunning = true;
            Debug.Log("[RouteTimerManager] Zamanlayýcý baþlatýldý.");
        }
    }

    /// <summary>
    /// Zamanlayýcýyý durdurur ve sonuçlarý yazdýrýr.
    /// </summary>
    public void StopTimer()
    {
        if (IsRunning)
        {
            IsRunning = false;
            Debug.Log($"[RouteTimerManager] Zamanlayýcý durduruldu. Toplam Süre: {ElapsedTime:F2} sn.");
            PrintResults();
        }
    }

    /// <summary>
    /// Belirtilen checkpoint ID'si için o anki zamaný kaydeder.
    /// </summary>
    public void LogCheckpointTime(string checkpointID)
    {
        if (IsRunning && !checkpointTimes.ContainsKey(checkpointID))
        {
            checkpointTimes.Add(checkpointID, ElapsedTime);
            Debug.Log($"[RouteTimerManager] LOG: '{checkpointID}' -> {ElapsedTime:F2} sn.");
        }
    }

    /// <summary>
    /// Zamanlayýcýyý ve tüm kayýtlarý ilk durumuna döndürür.
    /// </summary>
    public void ResetTimer()
    {
        IsRunning = false;
        ElapsedTime = 0f;
        checkpointTimes.Clear();
        Debug.Log("[RouteTimerManager] Zamanlayýcý sýfýrlandý.");
    }


    private void PrintResults()
    {
        Debug.Log("--- Checkpoint Süre Raporu ---");
        if (checkpointTimes.Count == 0)
        {
            Debug.Log("Kaydedilmiþ checkpoint bulunmuyor.");
        }
        else
        {
            foreach (var entry in checkpointTimes)
            {
                Debug.Log($"Nokta: {entry.Key} | Süre: {entry.Value:F2} sn");
            }
        }
        Debug.Log("---------------------------");
    }
}