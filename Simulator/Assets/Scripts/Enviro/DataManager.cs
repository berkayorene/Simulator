using UnityEngine;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour
{
    public VehicleFaults kazaScripti;
    public WeatherTracker havaScripti;

    public void TumunuKaydet()
    {
        // Kazaları ve hava durumunu kaydet
        if (kazaScripti != null)
            kazaScripti.KazalariKaydet();

        if (havaScripti != null)
            havaScripti.HavayiKaydet();

        // Aktif LevelData'yı al ve GameData’ya kaydet
        LevelHolder holder = FindObjectOfType<LevelHolder>();
        if (holder != null)
        {
            GameData.Instance.aktifLevelData = holder.thisLevelData;
            Debug.Log($"LevelData kaydedildi: {holder.thisLevelData.levelName}");
        }
        else
        {
            Debug.LogWarning("LevelHolder bulunamadı. LevelData atanamadı.");
        }

        Debug.Log("Tüm kayıtlar başarıyla alındı.");

        // EndScene'e geç
        SceneManager.LoadScene("EndScene");
    }
}
