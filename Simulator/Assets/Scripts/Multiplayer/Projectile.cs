using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    public float speed = 20f;
    public float lifeTime = 5f; // Mermi 5 saniye sonra yok olsun

    private Rigidbody rb;
    private ulong ownerClientId; // Mermiyi kimin ate�ledi�ini tutmak i�in

    private bool hasHit = false;

    public void SetOwner(ulong ownerId)
    {
        ownerClientId = ownerId;
    }

    public override void OnNetworkSpawn()
    {
        // Bu kod hem sunucuda hem de client'ta �al���r.
        rb = GetComponent<Rigidbody>();

        // Merminin hareketini SADECE sunucu belirler.
        if (IsServer)
        {
            rb.linearVelocity = transform.forward * speed;
            // Belirtilen s�re sonra mermiyi a� �zerinden yok et.
            Invoke(nameof(DestroyProjectile), lifeTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Sadece sunucuda çalışır VE eğer mermi daha önce birine çarpmadıysa devam eder.
        if (!IsServer || hasHit) return;

        // Kendi sahibine çarpmasını engelle
        var hitObjectNetworkId = other.gameObject.GetComponent<NetworkObject>()?.OwnerClientId;
        if (hitObjectNetworkId == ownerClientId)
        {
            return;
        }

        // Çarptığımız objenin bir TPSPlayerController'ı var mı diye kontrol et.
        if (other.gameObject.TryGetComponent<TPSPlayerController>(out var player))
        {
            // Bir oyuncuya çarptığı anda, kilidi hemen aktif et!
            hasHit = true;

            // Fiziksel etkileşimleri anında durdur
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            // Oyuncunun hasar alma fonksiyonunu çağır.
            Vector3 knockbackDirection = transform.forward;
            player.TakeHit(knockbackDirection);

            // Mermiyi yok et.
            DestroyProjectile();
        }
    }
    private void DestroyProjectile()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
