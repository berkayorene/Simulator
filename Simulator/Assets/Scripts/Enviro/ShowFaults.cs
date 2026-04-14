using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;

public class ShowFaults : MonoBehaviour
{
    public TMP_Text kazaTextUI;
    public TMP_Text havaTextUI;
    public TMP_Text levelInfoUI;

    void Start()
    {
        Debug.Log("KONTROL: EndScene yüklendi. GameData'daki hava dosya yolu: '" + GameData.Instance.sonHavaDosyaYolu + "'");

        // --- KAZA VERİLERİ ---
        string kazaDosyaYolu = GameData.Instance.sonKayitDosyaYolu;
        if (!string.IsNullOrEmpty(kazaDosyaYolu) && File.Exists(kazaDosyaYolu))
        {
            Dictionary<string, int> kazaSayilari = new Dictionary<string, int>();
            string[] satirlar = File.ReadAllLines(kazaDosyaYolu);

            foreach (string satir in satirlar)
            {
                int index = satir.IndexOf("] ");
                if (index != -1)
                {
                    string kazaTipi = satir.Substring(index + 2);
                    if (kazaSayilari.ContainsKey(kazaTipi))
                        kazaSayilari[kazaTipi]++;
                    else
                        kazaSayilari[kazaTipi] = 1;
                }
            }

            var kazaListesi = kazaSayilari.Select(kaza => $"{kaza.Key}: {kaza.Value}").ToList();
            kazaTextUI.text = string.Join(" | ", kazaListesi);
        }
        else
        {
            kazaTextUI.text = "Kaza yok.";
        }

        // --- HAVA DURUMU ---
        string havaDosyaYolu = GameData.Instance.sonHavaDosyaYolu;
        if (!string.IsNullOrEmpty(havaDosyaYolu) && File.Exists(havaDosyaYolu))
        {
            var havaTipleri = new HashSet<string>();
            string[] satirlar = File.ReadAllLines(havaDosyaYolu);

            foreach (string satir in satirlar)
            {
                int index = satir.IndexOf("] ");
                if (index != -1)
                {
                    string havaTipi = satir.Substring(index + 2);
                    havaTipleri.Add(havaTipi);
                }
            }

            havaTextUI.text = string.Join(" | ", havaTipleri);
        }
        else
        {
            havaTextUI.text = "Hava durumu değişmedi.";
        }

        // --- LEVEL BİLGİLERİ ---
        LevelData aktifLevelData = GameData.Instance.aktifLevelData;

        if (aktifLevelData != null)
        {
            levelInfoUI.text =
                $"Seviye: {aktifLevelData.levelName} | " +
                $"Görev: {aktifLevelData.missionType} | " +
                $"Noktalar: {aktifLevelData.TotalPointCount} | " +
                $"Toplam Mesafe: {aktifLevelData.TotalRouteLength:F1} m";
        }
        else
        {
            levelInfoUI.text = "Level bilgisi bulunamadı.";
        }
    }
}
