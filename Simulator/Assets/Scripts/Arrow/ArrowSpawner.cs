using UnityEngine;

public class ArrowSpawner : MonoBehaviour
{
    public Transform startPoint;         // Ok dizimine başlanacak yer
    public Transform endPoint;           // Gideceğin son hedef
    public GameObject arrowPrefab;       // Ok prefabı
    public float spacing = 2f;           // Oklar arası mesafe

    void Start()
    {
        Vector3 dir = (endPoint.position - startPoint.position).normalized;
        float dist = Vector3.Distance(startPoint.position, endPoint.position);
        int arrowCount = Mathf.FloorToInt(dist / spacing);

        for (int i = 0; i < arrowCount; i++)
        {
            Vector3 pos = startPoint.position + dir * spacing * i;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
            GameObject arrow = Instantiate(arrowPrefab, pos, rot);

            arrow.transform.parent = this.transform;
        }
    }
}
