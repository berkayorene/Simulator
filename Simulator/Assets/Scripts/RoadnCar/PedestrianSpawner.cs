using UnityEngine;

public class PedestrianSpawner : MonoBehaviour
{
    [Tooltip("Oluþturulacak yaya objesi (Prefab)")]
    [SerializeField] private GameObject pedestrian;

    [Tooltip("Yayanýn spawn olacaðý temel pozisyonu ve açýyý belirleyen transform")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Spawn noktasýnýn Z ekseninde (ileri yönde) ne kadar öteye spawn yapýlacaðýný belirler.")]
    [SerializeField] private float forwardDistance = 0f; // Varsayýlan olarak 0, yani eski davranýþýný korur.

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Gerekli objelerin atanýp atanmadýðýný kontrol et
            if (pedestrian == null)
            {
                Debug.LogError("Pedestrian objesi atanmamýþ!");
                return;
            }
            if (spawnPoint == null)
            {
                Debug.LogError("Spawn Point objesi atanmamýþ!");
                return;
            }

            Spawn();
        }
    }

    void Spawn()
    {
        // 1. Temel pozisyon olarak spawnPoint.position al.
        // 2. Buna, spawnPoint'in ileri yönü (mavi ok) ile forwardDistance çarpýmýný ekle.
        Vector3 finalSpawnPosition = spawnPoint.position + (spawnPoint.forward * forwardDistance);

        // Spawn iþlemini hesaplanan nihai pozisyonda ve spawnPoint'in rotasyonunda yap.
        Instantiate(pedestrian, finalSpawnPosition, spawnPoint.rotation);
        Debug.Log(pedestrian.name + " nesnesi oluþturuldu!");
    }
}