using UnityEngine;

public class MenuStartSound : MonoBehaviour
{
    public AudioSource audioSource;

    void Start()
    {
        if (audioSource != null)
        {
            audioSource.Play(); // Sahne açılır açılmaz sesi çal
        }
    }
}
