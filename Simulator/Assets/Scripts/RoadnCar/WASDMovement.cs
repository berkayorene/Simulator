using UnityEngine;

public class WASDMovement : MonoBehaviour
{
    // Karakterin ileri/geri hareket hızı
    [SerializeField] private float moveSpeed = 5.0f;

    // Karakterin kendi etrafında dönüş hızı
    [SerializeField] private float rotateSpeed = 100.0f;

    void Update()
    {
        // 1. Girdi Değerlerini Al
        // W ve S tuşları için: "Vertical" ekseni (+1 ile -1 arası bir değer verir)
        float verticalInput = Input.GetAxis("Vertical");

        // A ve D tuşları için: "Horizontal" ekseni (+1 ile -1 arası bir değer verir)
        float horizontalInput = Input.GetAxis("Horizontal");


        // 2. Hareketi Uygula (İleri/Geri)
        // Karakteri, baktığı yönde (ileri) 'verticalInput' değeri ile hareket ettirir.
        // W'ye basınca pozitif, S'ye basınca negatif değer alır ve böylece ileri/geri gider.
        transform.Translate(Vector3.forward * verticalInput * moveSpeed * Time.deltaTime);


        // 3. Rotasyonu Uygula (Sağa/Sola Dönme)
        // Karakteri, Y ekseni etrafında 'horizontalInput' değeri ile döndürür.
        // D'ye basınca pozitif, A'ya basınca negatif değer alır ve böylece sağa/sola döner.
        transform.Rotate(Vector3.up * horizontalInput * rotateSpeed * Time.deltaTime);
    }
}