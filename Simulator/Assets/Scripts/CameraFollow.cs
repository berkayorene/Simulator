using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;         // Takip edilecek karakter (küp)
    public Vector3 offset = new Vector3(0, 5f, -7f); // Kamera açısı
    public float followSpeed = 5f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.LookAt(target);
    }
}
