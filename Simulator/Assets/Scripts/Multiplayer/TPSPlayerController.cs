using UnityEngine;
using Unity.Netcode;

public class TPSPlayerController : NetworkBehaviour
{
    // --- HAREKET AYARLARI ---
    [Header("Hareket Ayarlar�")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float sprintSpeed = 8.0f;
    [SerializeField] private float jumpForce = 7.0f;

    [Header("Zıplama Zamanlayıcısı")]
    [SerializeField] private float jumpCooldown = 2.0f; // Zıplamalar arası bekleme süresi
    private float nextJumpTime = 0f; // Bir sonraki zıplamanın ne zaman yapılabileceğini tutar


    // --- KAMERA VE ROTASYON AYARLARI ---
    [Header("Kamera ve Rotasyon Ayarlar�")]
    [SerializeField] private float mouseSensitivity = 200.0f;
    [SerializeField] private Transform playerCameraTransform;

    private float cameraPitch = 0.0f;

    // --- ATE� ETME AYARLARI ---
    [Header("Ate� Etme Ayarlar�")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float knockbackForce = 15f;
    [SerializeField] private float fireCooldown = 1.0f; // YENİ EKLENDİ: Mermiler arası bekleme süresi (1 saniye)


    [Header("Görsel Ayarlar")]
    [SerializeField] private MeshRenderer playerMeshRenderer; // Rengi değişecek olan mesh renderer

    // --- A� DE���KENLER� ---
    private NetworkVariable<float> networkMass = new NetworkVariable<float>(1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> networkSpeedModifier = new NetworkVariable<float>(1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<Color> networkPlayerColor = new NetworkVariable<Color>(Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // YENİ EKLENDİ: Oyuncunun rengini senkronize etmek için

    // --- �ZEL DE���KENLER ---
    private Rigidbody rb;
    private bool isGrounded; // YEN�: Yerde olup olmad���m�z� kontrol etmek i�in
    private bool cursorIsLocked = true; // YEN�: Mouse'un kilitli olup olmad���n� takip etmek i�in
    private float nextFireTime = 0f; // YENİ EKLENDİ: Bir sonraki ateş etme zamanını tutar

    private Vector3 initialPosition; // �lk do�du�u pozisyonu saklamak i�in
    private Quaternion initialRotation; // �lk do�du�u rotasyonu saklamak i�in
    private const float DEFAULT_MASS = 1f;
    private const float MIN_MASS = 0.1f; // YENİ EKLENDİ: Minimum kütleyi bir sabite atamak daha temiz.
    private const float DEFAULT_SPEED_MODIFIER = 1f;
    private Color startColor = Color.white; // YENİ EKLENDİ
    private Color endColor = Color.red;   // YENİ EKLENDİ
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // YEN�: Start fonksiyonu mouse'u kilitlemek i�in eklendi
    void Start()
    {
        // Oyun ba�lad���nda mouse'u kilitle ve gizle
        SetCursorLock(cursorIsLocked);
    }

    public override void OnNetworkSpawn()
    {
        // Her zaman base metodunu en başta çağırmak iyi bir pratiktir.
        base.OnNetworkSpawn();

        // --- SADECE SUNUCUDA ÇALIŞACAK KOD ---
        // Sunucu, oyuna katılan her oyuncunun başlangıç pozisyonunu ve rotasyonunu kaydeder.
        // Bu, restart mekaniğinin doğru çalışması için kritiktir.
        if (IsServer)
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        // --- TÜM CLIENT'LARDA (SUNUCU DAHİL) ÇALIŞACAK KOD ---
        // Ağ değişkenleri (NetworkVariable) değiştiğinde, ilgili fonksiyonları çağırarak
        // görsel ve fiziksel güncellemeleri yap.

        // Kütle değiştiğinde OnMassChanged fonksiyonunu tetikle.
        networkMass.OnValueChanged += OnMassChanged;
        // Renk değiştiğinde OnColorChanged fonksiyonunu tetikle.
        networkPlayerColor.OnValueChanged += OnColorChanged;

        // Oyuncu ilk yaratıldığında, mevcut ağ değerlerini hemen uygula.
        OnMassChanged(0, networkMass.Value);
        OnColorChanged(Color.white, networkPlayerColor.Value);


        // --- SADECE OBJENİN SAHİBİNDE ÇALIŞACAK KOD ---
        if (IsOwner)
        {
            // Sadece kendi kameramızı aktif hale getiriyoruz.
            if (playerCameraTransform != null)
            {
                playerCameraTransform.gameObject.SetActive(true);
            }
        }
        // --- SAHİBİ OLMAYANLARDA ÇALIŞACAK KOD ---
        else
        {
            // Başka oyuncuların kamerasını kendi ekranımızda görmek istemeyiz, bu yüzden kapatıyoruz.
            if (playerCameraTransform != null)
            {
                playerCameraTransform.gameObject.SetActive(false);
            }
        }
    }

    // OnNetworkSpawn dışında, bu yardımcı fonksiyonlara da ihtiyacımız olacak:

    private void OnMassChanged(float previousValue, float newValue)
    {
        // Kütle değeri ağ üzerinden güncellendiğinde, Rigidbody'nin kütlesini ayarla.
        if (rb != null)
        {
            rb.mass = newValue;
        }
    }

    private void OnColorChanged(Color previousColor, Color newColor)
    {
        // Renk değeri ağ üzerinden güncellendiğinde, materyalin rengini ayarla.
        if (playerMeshRenderer != null)
        {
            playerMeshRenderer.material.color = newColor;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        // --- YEN�: MOUSE K�L�D�N� A�MA/KAPATMA ---
        // Escape tu�una bas�ld���nda mouse kilidini a�/kapat
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cursorIsLocked = !cursorIsLocked;
            SetCursorLock(cursorIsLocked);
        }

        // --- YEN�: ZIPLAMA KONTROL� ---
        // Yerde olup olmad���m�z� kontrol et
        CheckIfGrounded();

        // E�er yerdeysek ve Space tu�una bas�ld�ysa z�pla
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextJumpTime)
        {
            // Bir sonraki zıplama zamanını ayarla: mevcut zaman + bekleme süresi
            nextJumpTime = Time.time + jumpCooldown;

            // Zıplama kuvvetini uygula
            rb.AddForce(Vector3.up * jumpForce * rb.mass, ForceMode.Impulse);
        }

        // --- ATE� ETME KODU ---
        if (Input.GetMouseButtonDown(0) && cursorIsLocked && Time.time >= nextFireTime)
        {
            // Bir sonraki ateş etme zamanını ayarla: mevcut zaman + bekleme süresi
            nextFireTime = Time.time + fireCooldown;

            // Sunucudan bizim için bir mermi fırlatmasını istiyoruz.
            ShootServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        }

        // --- HAREKET VE ROTASYON KODLARI ---
        HandleRotation();
        HandleMovement();
    }

    [ServerRpc]
    public void RequestRestartServerRpc()
    {
        // Sunucu, oyundaki t�m oyuncular�n listesini al�r.
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            // Her bir oyuncunun obje referans�n� bulur.
            NetworkObject playerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            if (playerObj != null)
            {
                // Oyuncunun script'indeki s�f�rlama fonksiyonunu sunucu taraf�nda �a��r�r.
                playerObj.GetComponent<TPSPlayerController>().ResetPlayerState();
            }
        }
    }

    public void ResetPlayerState()
    {
        // Sadece sunucu bu de�i�kenleri de�i�tirebilir.
        networkMass.Value = DEFAULT_MASS;
        networkSpeedModifier.Value = DEFAULT_SPEED_MODIFIER;

        // �imdi, bu oyuncunun Client'�na kendini ���nlamas� i�in bir komut g�nder.
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { OwnerClientId }
            }
        };
        TeleportClientRpc(initialPosition, initialRotation, clientRpcParams);
    }

    [ClientRpc]
    private void TeleportClientRpc(Vector3 pos, Quaternion rot, ClientRpcParams clientRpcParams = default)
    {
        // Client, kendi karakterini ���nlar.
        // Client-Authoritative oldu�u i�in bu en g�venli yoldur.
        transform.position = pos;
        transform.rotation = rot;

        // Fizik motorunun kafas�n�n kar��mamas� i�in h�z� ve d�n��� de s�f�rla.
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }


    // YEN�: Kodun daha temiz olmas� i�in rotasyon ve hareketi kendi fonksiyonlar�na ta��d�k.
    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);
        playerCameraTransform.localEulerAngles = new Vector3(cameraPitch, 0, 0);
    }

    private void HandleMovement()
    {
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = (isSprinting ? sprintSpeed : moveSpeed) * networkSpeedModifier.Value;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;

        // Hareketi Rigidbody ile yapmak daha iyi olabilir ama �imdilik bu �ekilde kalabilir.
        transform.position += moveDirection.normalized * currentSpeed * Time.deltaTime;
    }

    // YEN�: Yerde olup olmad���m�z� kontrol eden fonksiyon
    private void CheckIfGrounded()
    {
        // Oyuncunun aya��n�n alt�ndan a�a�� do�ru �ok k�sa bir ���n g�ndererek yere de�ip de�medi�ini kontrol ediyoruz.
        float extraHeight = 0.1f;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, extraHeight);
    }

    // YEN�: Mouse durumunu ayarlayan yard�mc� fonksiyon
    private void SetCursorLock(bool isLocked)
    {
        if (isLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // --- RPC VE HASAR ALMA FONKS�YONLARI BURADA DE���MEDEN KALIYOR ---
    [ServerRpc]
    private void ShootServerRpc(Vector3 spawnPos, Quaternion spawnRot)
    {
        GameObject projectileInstance = Instantiate(projectilePrefab, spawnPos, spawnRot);
        NetworkObject netObj = projectileInstance.GetComponent<NetworkObject>();
        netObj.Spawn(true);
        projectileInstance.GetComponent<Projectile>().SetOwner(OwnerClientId);
    }

    public void TakeHit(Vector3 knockbackDirection)
    {
        // Bu fonksiyon her zaman sunucuda çalışır.

        // 1. Statü güncellemeleri (NetworkVariable'lar) sunucu tarafından yapılır.
        networkMass.Value -= 0.1f;
        networkSpeedModifier.Value -= 0.05f;
        if (networkMass.Value < MIN_MASS) networkMass.Value = MIN_MASS;
        if (networkSpeedModifier.Value < 0.2f) networkSpeedModifier.Value = 0.2f;

        float healthPercent = (networkMass.Value - MIN_MASS) / (DEFAULT_MASS - MIN_MASS);
        networkPlayerColor.Value = Color.Lerp(endColor, startColor, healthPercent);

        // 2. KİM OLURSA OLSUN (HOST YA DA CLIENT), SAHİBİNE RPC GÖNDER.
        // Herkes için tek bir kural var: Sunucu, vurulan oyuncuya RPC ile komut gönderir.
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { OwnerClientId }
            }
        };
        ApplyKnockbackClientRpc(knockbackDirection, clientRpcParams);
    }

    // ApplyKnockbackClientRpc fonksiyonu aynı kalıyor, ona dokunmayın.
    [ClientRpc]
    private void ApplyKnockbackClientRpc(Vector3 knockbackDirection, ClientRpcParams clientRpcParams = default)
    {
        if (rb != null)
        {
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        }

    }
}