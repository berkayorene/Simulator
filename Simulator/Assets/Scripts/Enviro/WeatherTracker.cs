using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WeatherTracker : MonoBehaviour
{
    private List<string> geciciHavaDegisiklikleri = new List<string>();
    public string kullaniciAdi = "Player";  // İstersen kullanıcı adını özelleştir

    public static WeatherTracker Instance;

    private void Awake()
    {
        // Singleton oluştur
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Hava durumu değişince çağır
    public void HavaEkle(string havaTipi)
    {
        // (EKLENDİ) Bu satırı ekleyin
        Debug.Log("HavaEkle fonksiyonu çalıştı. Eklenen hava: " + havaTipi);

        string zaman = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string logSatiri = $"[{zaman}] {havaTipi}";
        geciciHavaDegisiklikleri.Add(logSatiri);
    }

    // Kayıt işlemi
    public void HavayiKaydet()
    {
        if (geciciHavaDegisiklikleri.Count == 0)
        {
            Debug.Log("Kaydedilecek hava değişikliği yok.");
            return;
        }

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string dosyaAdi = $"hava_loglari_{kullaniciAdi}_{timestamp}.txt";
        string dosyaYolu = Path.Combine(Application.persistentDataPath, dosyaAdi);

        File.WriteAllLines(dosyaYolu, geciciHavaDegisiklikleri);
        Debug.Log($"Hava dosyası yazıldı: {dosyaYolu}");

        // ✅ Burayı değiştirdik
        GameData.Instance.sonHavaDosyaYolu = dosyaYolu;

        geciciHavaDegisiklikleri.Clear();
    }
}
