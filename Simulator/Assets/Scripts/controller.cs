using UnityEngine;

public class SimplePlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A-D tuşları
        float v = Input.GetAxisRaw("Vertical");   // W-S tuşları

        Vector3 move = new Vector3(h, 0, v).normalized * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);
    }
}
