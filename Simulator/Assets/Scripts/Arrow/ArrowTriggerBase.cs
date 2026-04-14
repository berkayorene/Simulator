using System.Collections.Generic;
using UnityEngine;

public class ArrowTriggerBase : MonoBehaviour
{
    public GameObject arrowPrefab;
    public Transform player;
    public float triggerDistance = 3f;
    public bool triggered = false;

    protected virtual void Update()
    {
        if (triggered || player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        if (dist < triggerDistance)
        {
            SpawnArrows();
            triggered = true;
        }
    }

    protected virtual void SpawnArrows()
    {
        // override in child classes
    }

    protected void SpawnArrowLine(Vector3 startPos, Vector3 direction, int count, float spacing)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = startPos + direction.normalized * spacing * i + Vector3.up * 0.1f;
            Quaternion rot = Quaternion.LookRotation(direction);
            Instantiate(arrowPrefab, pos, rot);
        }
    }
}
