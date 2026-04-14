using UnityEngine;

public class GameSessionManager : MonoBehaviour
{
    // Singleton deseni için statik referans
    public static GameSessionManager Instance { get; private set; }

    // Sahneler arasý taþýnacak olan veri
    public RunSummaryData LastRunSummary { get; set; }

    private void Awake()
    {
        // Eðer daha önce bir Instance oluþturulmadýysa, bu nesneyi Instance yap.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Bu nesnenin sahne deðiþiminde yok olmasýný engelle.
        }
        // Eðer zaten bir Instance varsa, bu yeni kopyayý yok et.
        else
        {
            Destroy(gameObject);
        }
    }
}