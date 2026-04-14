using UnityEngine;

public class Player_CpCollector : MonoBehaviour
{
    [SerializeField] private CPManager manager;
    void OnTriggerEnter(Collider other)
    {
        CPInstance checkpoint = other.GetComponent<CPInstance>();

        if (checkpoint != null && manager != null)
        {
            manager.CheckpointHit(checkpoint);
        }
    }

}
