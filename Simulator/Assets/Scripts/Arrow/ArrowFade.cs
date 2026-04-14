using UnityEngine;

public class ArrowFade : MonoBehaviour
{
    public Transform player;
    public float fadeTriggerDistance = 1.5f;
    private Material mat;
    private bool faded = false;

    void Start()
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            mat = rend.material;
        }

        // Otomatik olarak oyuncuyu bul
        if (player == null)
        {
            GameObject go = GameObject.FindWithTag("Player");
            if (go != null)
                player = go.transform;
        }
    }

    void Update()
    {
        if (faded || player == null || mat == null)
            return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < fadeTriggerDistance)
        {
            Color c = mat.color;
            c.a = 0f; // direkt şeffaf yap
            mat.color = c;

            faded = true; // sadece bir kere çalışsın
        }
    }
}
