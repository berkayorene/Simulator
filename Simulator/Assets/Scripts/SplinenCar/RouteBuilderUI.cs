using UnityEngine;
using System.Collections.Generic;

public class RouteBuilderUI : MonoBehaviour
{
    /*[Tooltip("Ayarlanan rotayý alacak olan RouteManager objesini buraya sürükleyin.")]
    [SerializeField] private RouteManager routeManager;


    [Header("Live Route Preview")]
    [Tooltip("Oluþturulan rotanýn anlýk ID listesi. (Sadece Görüntüleme Amaçlý)")]
    
    // Týklamalarla oluþturulan rotanýn ID'lerinin geçici olarak tutulduðu liste.
    [SerializeField]
    private List<string> currentRouteIDs = new List<string>();

    /// <summary>
    /// Bir ISPoint butonuna týklandýðýnda çaðrýlýr.
    /// </summary>
    public void AddPointToRoute(ISPointButton button)
    {
        if (button == null || string.IsNullOrEmpty(button.PointID))
        {
            Debug.LogWarning("Butonda ISPointButton veya PointID bulunamadý!");
            return;
        }

        currentRouteIDs.Add(button.PointID);
        // UI'da veya konsolda kullanýcýya geri bildirim vermek için:
        Debug.Log($"Nokta eklendi: {button.PointID}. Mevcut Rota: {string.Join(" -> ", currentRouteIDs)}");
    }

    /// <summary>
    /// Geçici olarak oluþturulan rotayý temizler.
    /// </summary>
    public void ClearRoute()
    {
        currentRouteIDs.Clear();
        Debug.Log("Mevcut rota temizlendi. Yeni bir rota oluþturabilirsiniz.");

                if (routeManager != null)
        {
            // RouteManager'a eklediðimiz yeni public metodu çaðýrýyoruz.
            routeManager.FullReset();
        }
        else
        {
            Debug.LogWarning("RouteManager referansý atanmamýþ, bu yüzden sadece UI temizlendi.");
        }
    }

    /// <summary>
    /// Geçici rotanýn son eklenen noktasýný siler.
    /// </summary>
    public void RemoveLastPoint()
    {
        // Eðer listede en az bir eleman varsa...
        if (currentRouteIDs.Count > 0)
        {
            // Listenin son elemanýnýn index'ini bul (Count - 1).
            int lastIndex = currentRouteIDs.Count - 1;
            string removedPointID = currentRouteIDs[lastIndex];

            // Son elemaný listeden kaldýr.
            currentRouteIDs.RemoveAt(lastIndex);

            // Konsola güncel durumu yazdýr.
            Debug.Log($"Son nokta silindi: {removedPointID}. Mevcut Rota: {string.Join(" -> ", currentRouteIDs)}");
        }
        else
        {
            Debug.LogWarning("Silinecek nokta bulunmuyor. Rota zaten boþ.");
        }
    }



    /// <summary>
    /// Rota oluþturma iþlemini bitirir ve veriyi RouteManager'a göndererek rotayý kurar.
    /// </summary>
    public void ApplyRouteToManager()
    {
        if (routeManager == null)
        {
            Debug.LogError("RouteManager referansý atanmamýþ! Lütfen Inspector'dan atama yapýn.");
            return;
        }

        if (currentRouteIDs.Count < 2)
        {
            Debug.LogWarning("Bir rota en az 2 noktadan oluþmalýdýr.");
            return;
        }

        // RouteManager'daki yeni metodumuzu çaðýrarak listeyi gönderiyor ve rotayý kurmasýný istiyoruz.
        routeManager.SetAndInitializeRoute(currentRouteIDs);

        Debug.Log("Rota baþarýyla RouteManager'a gönderildi ve kuruldu!");
    }*/
}