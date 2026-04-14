using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;

public class NetworkDiscoveryHost : MonoBehaviour
{
    // Host'un yayýn yapacaðý port
    public int discoveryPort = 9090;
    // Ne kadar sürede bir yayýn yapýlacaðý (saniye)
    public float broadcastInterval = 2f;

    private UdpClient udpClient;

    void Start()
    {
        // UDP client'ý baþlat
        udpClient = new UdpClient();
        // Broadcast için gerekli ayarý yap
        udpClient.EnableBroadcast = true;
        // Periyodik olarak anons yapmaya baþla
        StartCoroutine(BroadcastPresence());
    }

    private IEnumerator BroadcastPresence()
    {
        // Bu script aktif olduðu sürece anons yapmaya devam et
        while (enabled)
        {
            // Aðdaki diðer cihazlarýn oyunumuzu tanýmasý için özel bir mesaj
            // Bu mesajý daha sonra server adý, oyuncu sayýsý gibi bilgilerle zenginleþtirebiliriz.
            string message = "KupOyunum_Host_Anonsu";
            byte[] data = Encoding.UTF8.GetBytes(message);

            // Broadcast adresi (255.255.255.255), aðdaki herkese mesaj gönderir
            IPEndPoint broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, discoveryPort);

            try
            {
                udpClient.Send(data, data.Length, broadcastEndpoint);
                //Debug.Log("Host anonsu yapýldý.");
            }
            catch (SocketException e)
            {
                Debug.LogError("Broadcast hatasý: " + e.Message);
            }

            // Belirtilen süre kadar bekle
            yield return new WaitForSeconds(broadcastInterval);
        }
    }

    void OnDisable()
    {
        // Script devre dýþý kaldýðýnda UDP client'ý kapat
        if (udpClient != null)
        {
            udpClient.Close();
            udpClient = null;
        }
    }
}