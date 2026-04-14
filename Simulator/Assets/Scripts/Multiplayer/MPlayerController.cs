using UnityEngine;
using Unity.Netcode; // Netcode kullanacaðýmýzý belirtiyoruz!

public class MPlayerController : NetworkBehaviour // MonoBehaviour yerine NetworkBehaviour kullanýyoruz!
{
    public float moveSpeed = 5f;

    void Update()
    {
        // BU EN ÖNEMLÝ KISIM!
        // Kodun sadece bu objenin "sahibi" olan client'ta çalýþmasýný saðlar.
        // Yani Player 1 sadece kendi küpünü, Player 2 sadece kendi küpünü kontrol edebilir.
        // Bizim senaryomuzda Player 2'nin küpü olmayacaðý için bu kod onda hiç çalýþmaz.
        if (!IsOwner) return;

        // --- Buradan sonrasý standart hareket kodu ---

        float horizontalInput = Input.GetAxis("Horizontal"); // A ve D tuþlarý
        float verticalInput = Input.GetAxis("Vertical");     // W ve S tuþlarý

        Vector3 moveDirection = new Vector3(horizontalInput, 0, verticalInput);
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}