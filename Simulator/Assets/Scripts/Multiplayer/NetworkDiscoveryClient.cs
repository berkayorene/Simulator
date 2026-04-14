using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent; // Thread-safe koleksiyon için

public class NetworkDiscoveryClient : MonoBehaviour
{
    public int discoveryPort = 9090;
    private UdpClient udpClient;

    // Ana thread'in güvenle okuyabilmesi için thread-safe bir kuyruk
    public static ConcurrentQueue<string> foundServerIPs = new ConcurrentQueue<string>();

    void Start()
    {
        try
        {
            udpClient = new UdpClient(discoveryPort);
            udpClient.BeginReceive(OnUdpData, null);
            Debug.Log("Sunucu dinlemesi baþlatýldý...");
        }
        catch (Exception e)
        {
            Debug.LogError("UDP Client baþlatýlamadý: " + e.Message);
        }
    }

    private void OnUdpData(IAsyncResult result)
    {
        try
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] receivedBytes = udpClient.EndReceive(result, ref remoteEP);
            string message = Encoding.UTF8.GetString(receivedBytes);

            if (message == "KupOyunum_Host_Anonsu")
            {
                // Bulunan IP'yi direkt kullanmak yerine kuyruða ekle
                foundServerIPs.Enqueue(remoteEP.Address.ToString());
            }

            udpClient.BeginReceive(OnUdpData, null);
        }
        catch { /* Hatalarý yoksay */ }
    }

    void OnDisable()
    {
        if (udpClient != null)
        {
            udpClient.Close();
            udpClient = null;
        }
    }
}