using UnityEngine;

public class EnviroCamOffset : MonoBehaviour
{
    public Transform bus;                // Otobüs objesi
    public Transform effect;             // Enviro'nun effects objesi (runtime'da atanabilir)
    public string effectObjectName = "Effects"; // Enviro’nun oluşturduğu objenin adı
    public Vector3 offset = new Vector3(0f, 0f, 8f);

    void Start()
    {
        // Oyun başladığında effect objesini sahnede otomatik bul
        if (effect == null)
        {
            GameObject effectGO = GameObject.Find(effectObjectName);
            if (effectGO != null)
            {
                effect = effectGO.transform;
            }
            else
            {
                Debug.LogWarning("EnviroCamOffset: 'Effects' objesi sahnede bulunamadı!");
            }
        }
    }

    void LateUpdate()
    {
        if (bus == null || effect == null)
            return;

        Vector3 forwardOffset = bus.forward * offset.z + Vector3.up * offset.y + bus.right * offset.x;
        effect.position = bus.position + forwardOffset;
    }
}
