using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;
using System.Collections.Generic;

public class NetworkUI : MonoBehaviour
{
    [Header("Connection Buttons")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TMP_InputField ipAddressInput;

    [Header("Discovery")]
    [SerializeField] private Button findServersButton;
    [SerializeField] private GameObject serverListContent;
    [SerializeField] private GameObject serverButtonPrefab;

    [Header("Game Controls")]
    [SerializeField] private Button restartButton;

    [Header("Discovery Scripts")]
    [SerializeField] private NetworkDiscoveryHost discoveryHost;
    [SerializeField] private NetworkDiscoveryClient discoveryClient;

    private HashSet<string> foundServers = new HashSet<string>();

    private void Awake()
    {
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClientWithInput);
        findServersButton.onClick.AddListener(FindServers);
        restartButton.onClick.AddListener(OnRestartButtonClicked);

    }

    void Update()
    {
        while (NetworkDiscoveryClient.foundServerIPs.TryDequeue(out string ip))
        {
            if (!foundServers.Contains(ip))
            {
                foundServers.Add(ip);
                CreateServerButton(ip);
            }
        }
    }

    private void StartHost()
    {
        Debug.Log("HOST BAÞLATILIYOR...");
        NetworkManager.Singleton.StartHost();
        discoveryHost.enabled = true;
        Debug.Log("Host baþlatýldý ve anons yapýyor.");
    }

    private void StartClientWithInput()
    {
        string ipAddress = ipAddressInput.text;
        ConnectClient(ipAddress);
    }

    private void FindServers()
    {
        foundServers.Clear();
        foreach (Transform child in serverListContent.transform)
        {
            Destroy(child.gameObject);
        }
        discoveryClient.enabled = true;
        Debug.Log("Sunucu arama aktif.");
    }

    private void CreateServerButton(string ipAddress)
    {
        // --- KONTROL NOKTASI 1 ---
        Debug.Log($"Arayüz için buton oluþturuluyor. IP: {ipAddress}");

        GameObject buttonObj = Instantiate(serverButtonPrefab, serverListContent.transform);
        buttonObj.GetComponentInChildren<TMP_Text>().text = ipAddress;
        buttonObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            // --- KONTROL NOKTASI 2 ---
            Debug.Log($"Oluþturulan sunucu butonuna týklandý! IP: {ipAddress}");
            ConnectClient(ipAddress);
        });
    }

    private void ConnectClient(string ipAddress)
    {
        // --- KONTROL NOKTASI 3 ---
        Debug.Log($"ConnectClient fonksiyonu çaðrýldý. Hedef IP: {ipAddress}");

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ipAddress, 7777);

        // --- KONTROL NOKTASI 4 ---
        Debug.Log($"NetworkManager transport ayarlandý. Baðlantý denemesi baþlýyor...");

        NetworkManager.Singleton.StartClient();
    }

    private void OnRestartButtonClicked()
    {
        // Eðer oyuna baðlý bir oyuncuysak...
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            // Kendi oyuncu kontrolcümüzü bul ve sunucudan restart isteðinde bulunmasýný söyle.
            TPSPlayerController localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<TPSPlayerController>();
            if (localPlayer != null)
            {
                localPlayer.RequestRestartServerRpc();
            }
        }
    }

}