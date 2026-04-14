using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VehicleFaults : MonoBehaviour
{
    private List<string> geciciKazalar = new List<string>();
    public string kullaniciAdi = "Ali";

    // Kazaları geçici olarak ekle
    public void KazaEkle(string kazaTipi)
    {
        string zaman = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string logSatiri = $"[{zaman}] {kazaTipi}";
        geciciKazalar.Add(logSatiri);
    }

    // Kaydet butonuna tıklanınca çağrılacak
    public void KazalariKaydet()
    {
        if (geciciKazalar.Count == 0) return;

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string dosyaAdi = $"kaza_loglari_{kullaniciAdi}_{timestamp}.txt";
        string dosyaYolu = Path.Combine(Application.persistentDataPath, dosyaAdi);

        File.WriteAllLines(dosyaYolu, geciciKazalar);

        // DİKKAT: Dosya yolunu saklıyoruz!
        GameData.Instance.sonKayitDosyaYolu = dosyaYolu;

        geciciKazalar.Clear();

        //SceneManager.LoadScene("EndScene");


    }
}
