using UnityEngine;

public class PedestrianController : MonoBehaviour
{
    private Vector3 targetPosition;
    private float moveSpeed;
    private bool isInitialized = false;


    // YEN� EKLEND�: UI Manager'a referans tutmak i�in.
    private PedestrianInteraction interactionUI;

    private void Awake()
    {
        // Sahnedeki UI y�neticisini oyun ba��nda bul ve referans�n� al.
        // Bu, her yayan�n tek tek atanmas�na gerek kalmadan sistemi bulmas�n� sa�lar.
        interactionUI = FindObjectOfType<PedestrianInteraction>();
    }

    public void Initialize(Vector3 targetPos, float speed)
    {
        this.targetPosition = targetPos;
        this.moveSpeed = speed;
        this.isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            Destroy(gameObject);
        }
    }

    // --- YEN� EKLEND�: �arp��ma alg�lama metodu ---
    private void OnTriggerEnter(Collider other)
    {
        // �arpan nesnenin etiketinin "Player" olup olmad���n� kontrol et.
        if (other.GetComponentInParent<BusIdentifier>() != null)
        {
            // E�er UI y�neticisi bulunduysa, uyar� g�sterme fonksiyonunu �a��r.
            if (interactionUI != null)
            {
                interactionUI.ShowWarning();
            }
            else
            {
                Debug.LogWarning("Sahnede PedestrianInteractionUI script'i bulunamad�.");
            }

        }
    }
}