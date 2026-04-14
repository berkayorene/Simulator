using UnityEngine;
using Unity.Netcode;

public class LobbyCameraManager : MonoBehaviour
{
    void Start()
    {
        // NetworkManager'a, bir client baðlandýðýnda veya bir host baþladýðýnda
        // OnClientStarted fonksiyonunu çalýþtýrmasýný söyle.
        NetworkManager.Singleton.OnClientStarted += HandleClientStarted;
    }

    private void HandleClientStarted()
    {
        // Eðer biz bir oyuncu olarak oyuna baþarýyla baðlandýysak
        // (ister host, ister client olalým fark etmez),
        // bu lobi kamerasýnýn artýk bir iþi kalmamýþtýr. Onu kapat.
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        // Obje yok olduðunda event aboneliðini sonlandýrmak önemlidir.
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientStarted -= HandleClientStarted;
        }
    }
}