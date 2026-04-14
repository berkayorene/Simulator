using UnityEngine;
using UnityEngine.AI;

public enum PedestrianDirection { SoldanSaga, SagdanSola }

public class PedestrianScenarioManager : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private RouteManager routeManager;
    [SerializeField] private GameObject pedestrianPrefab;

    [Header("Senaryo Ayarları")]
    [SerializeField] private float spawnDistanceAhead = 50f;

    [Header("Yaya Davranış Ayarları")]
    [SerializeField] private PedestrianDirection crossingDirection = PedestrianDirection.SoldanSaga;
    [SerializeField] private float startOffset = 7f;
    [SerializeField] private float endOffset = 7f;
    [SerializeField] private float crossingSpeed = 1.5f;

    public void TriggerPedestrianCrossing()
    {
        if (routeManager == null || pedestrianPrefab == null)
        {
            Debug.LogError("RouteManager veya PedestrianPrefab atanmamış!", this.gameObject);
            return;
        }

        float targetAbsoluteDistance = routeManager.TotalDistanceTraveled + spawnDistanceAhead;

        if (routeManager.TryGetPointAndDirectionAtDistance(targetAbsoluteDistance, out Vector3 pointOnSpline, out Vector3 splineForwardDirection))
        {
            CalculateAndSpawnPedestrian(pointOnSpline, splineForwardDirection);
        }
        else
        {
            Debug.LogError("Yaya geçidi oluşturulacak mesafe, rotanın kalan toplam uzunluğunu aşıyor!", this.gameObject);
        }
    }

    private void CalculateAndSpawnPedestrian(Vector3 pointOnSpline, Vector3 splineForwardDirection)
    {
        Vector3 perpendicularRight = Vector3.Cross(splineForwardDirection, Vector3.up).normalized;

        Vector3 startPoint;
        Vector3 endPoint;

        if (crossingDirection == PedestrianDirection.SoldanSaga)
        {
            startPoint = pointOnSpline - perpendicularRight * startOffset;
            endPoint = pointOnSpline + perpendicularRight * endOffset;
        }
        else
        {
            startPoint = pointOnSpline + perpendicularRight * startOffset;
            endPoint = pointOnSpline - perpendicularRight * endOffset;
        }

        // NavMesh üzerinde en yakın geçerli pozisyonu bulmaya çalış
        if (NavMesh.SamplePosition(startPoint, out NavMeshHit navStart, 2f, NavMesh.AllAreas) &&
            NavMesh.SamplePosition(endPoint, out NavMeshHit navEnd, 2f, NavMesh.AllAreas))
        {
            GameObject pedestrianInstance = Instantiate(pedestrianPrefab, navStart.position, Quaternion.LookRotation(navEnd.position - navStart.position));

            if (pedestrianInstance.TryGetComponent<PedestrianController>(out var controller))
            {
                controller.Initialize(navEnd.position, crossingSpeed);
            }
            else
            {
                Debug.LogError($"'{pedestrianPrefab.name}' prefab'ında PedestrianController script'i bulunamadı!", pedestrianInstance);
            }

            Debug.DrawLine(navStart.position, navEnd.position, Color.red, 5f);
            Debug.DrawRay(pointOnSpline, splineForwardDirection * 10f, Color.blue, 5f);
            Debug.DrawRay(pointOnSpline, perpendicularRight * 5f, Color.green, 5f);
        }
        else
        {
            Debug.LogWarning("Yaya için geçerli bir NavMesh pozisyonu bulunamadı.");
        }
    }
}
