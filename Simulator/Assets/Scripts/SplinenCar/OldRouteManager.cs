/*

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class RouteManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private RouteTimerManager routeTimerManager;
    [Tooltip("Konumu takip edilecek olan þoförün (otobüsün) Collider'ý.")]
    [SerializeField] private Collider playerCollider;

    [Header("Baking Target (Editor Only)")]
    [SerializeField] private LevelData levelDataToBake;
    public LevelData LevelDataToBake => levelDataToBake;

    [Header("Route Definitions")]
    [Tooltip("Seviyenin en baþýnda belirlenen, asla deðiþmeyen orijinal rota.")]
    [SerializeField] private List<string> initialRoute;
    public IReadOnlyList<string> InitialRoute => initialRoute;
    public List<string> RoutePointIDs => initialRoute;

    [Tooltip("Admin tarafýndan belirlenen, rotanýn OLMASI GEREKEN NÝHAÝ halidir.")]
    [field: SerializeField, ReadOnly]
    private List<string> liveRoute = new List<string>();
    public IReadOnlyList<string> LiveRoute => liveRoute;

    [Tooltip("Þoförün geri dönüþler hariç net ilerlemesini gösteren rota.")]
    [field: SerializeField, ReadOnly]
    private List<string> netProgressRoute = new List<string>();
    public IReadOnlyList<string> NetProgressRoute => netProgressRoute;

    [Tooltip("Þoförün geçtiði her bir noktanýn tam seyahat kaydý.")]
    [field: SerializeField, ReadOnly]
    private List<string> totalTravelLogRoute = new List<string>();
    public IReadOnlyList<string> TotalTravelLogRoute => totalTravelLogRoute;

    // --- Private Route State ---
    private List<string> driverJourneyPlanIDs = new List<string>();
    private List<ISPoint> driverJourneyPoints = new List<ISPoint>();
    private List<ISSpline> driverJourneySplines = new List<ISSpline>();

    private bool timerStarted = false;
    private float distanceTraveledOnPreviousSplines = 0f;
    private bool isBacktracking = false;
    private int backtrackStepsRemaining = 0;
    private List<string> backtrackPath = new List<string>(); // Track the exact backtrack sequence

    // --- Public Properties ---
    public int CurrentJourneyStepIndex { get; private set; } = 0;
    public bool RouteInitialized { get; private set; } = false;
    public bool RouteFinished { get; private set; } = false;
    public float TotalRouteLength { get; private set; } = 0f;
    public float TotalDistanceTraveled { get; private set; } = 0f;
    public Vector3 ClosestPointOnRoute { get; private set; }

    private void Start()
    {
        if (playerCollider == null)
        {
            Debug.LogError("Player Collider atanmamýþ! Baþlangýç noktasý kontrolü yapýlamaz.", this);
        }

        if (initialRoute != null && initialRoute.Count > 1)
        {
            InitializeRoute();
        }
        else
        {
            Debug.LogWarning("RouteManager'da 'Initial Route' tanýmlanmamýþ. Rota kurulamýyor.", this);
        }
    }

    private void Update()
    {
        if (RouteInitialized && !timerStarted && Input.anyKeyDown)
        {
            routeTimerManager?.StartTimer();
            timerStarted = true;
        }
    }

    public void UpdateLiveRoute(List<string> newAdminRoute)
    {
        if (newAdminRoute == null || newAdminRoute.Count < 2)
        {
            Debug.LogWarning("Geçersiz yeni rota talebi.");
            return;
        }

        // Validate the new route
        if (!ValidateRoute(newAdminRoute))
        {
            Debug.LogError($"Geçersiz rota! Rota uygulanamadý: {string.Join("->", newAdminRoute)}");
            return;
        }

        // Check if starting point is the same
        if (liveRoute.Count > 0 && newAdminRoute[0] != liveRoute[0])
        {
            Debug.LogError($"Baþlangýç noktasý deðiþtirilemiyor! Eski: {liveRoute[0]}, Yeni: {newAdminRoute[0]}");
            return;
        }

        this.liveRoute = new List<string>(newAdminRoute);
        Debug.Log($"Admin Emri Alýndý: Yeni LiveRoute -> {string.Join("->", liveRoute)}");

        CalculateAndApplyNewJourneyPlan();
    }

    private bool ValidateRoute(List<string> routeToValidate)
    {
        if (routeToValidate == null || routeToValidate.Count < 2)
            return false;

        if (!TryBuildPointMap(out var allPointsInScene))
            return false;

        // Check if all points exist
        foreach (var pointID in routeToValidate)
        {
            if (!allPointsInScene.ContainsKey(pointID))
            {
                Debug.LogError($"'{pointID}' ID'li ISPoint sahnede bulunamadý!");
                return false;
            }
        }

        // Check connections between consecutive points
        for (int i = 0; i < routeToValidate.Count - 1; i++)
        {
            ISPoint currentPoint = allPointsInScene[routeToValidate[i]];
            ISPoint nextPoint = allPointsInScene[routeToValidate[i + 1]];

            if (FindSplineBetween(currentPoint, nextPoint) == null)
            {
                Debug.LogError($"'{routeToValidate[i]}' ve '{routeToValidate[i + 1]}' arasýnda baðlantý bulunamadý!");
                return false;
            }
        }

        return true;
    }

    private void CalculateAndApplyNewJourneyPlan()
    {
        if (netProgressRoute.Count == 0)
        {
            Debug.Log("NetProgressRoute boþ, ilk rota olarak ayarlanýyor.");
            driverJourneyPlanIDs = new List<string>(liveRoute);
            isBacktracking = false;
            backtrackStepsRemaining = 0;
            backtrackPath.Clear();
            RebuildAndActivateRoute();
            return;
        }

        string currentLocation = netProgressRoute.LastOrDefault();
        if (currentLocation == null)
        {
            Debug.LogError("Mevcut konum bulunamadý, yeni plan hesaplanamýyor.");
            return;
        }

        Debug.Log("--- Yeni Yolculuk Planý Hesaplanýyor ---");
        Debug.Log($"Mevcut Konum: {currentLocation}");
        Debug.Log($"Net Ýlerleme Yolu (NPR): {string.Join("->", netProgressRoute)}");
        Debug.Log($"Yeni Hedef Rota (LR): {string.Join("->", liveRoute)}");

        // Find the last common point between netProgressRoute and liveRoute
        int lastCommonIndex = -1;
        int maxCompareLength = Mathf.Min(netProgressRoute.Count, liveRoute.Count);

        for (int i = 0; i < maxCompareLength; i++)
        {
            if (netProgressRoute[i] == liveRoute[i])
            {
                lastCommonIndex = i;
            }
            else
            {
                break;
            }
        }

        if (lastCommonIndex == -1)
        {
            Debug.LogError("Yeni rota ile ortak nokta bulunamadý!");
            return;
        }

        string commonPoint = liveRoute[lastCommonIndex];
        Debug.Log($"Son ortak nokta: {commonPoint} (index: {lastCommonIndex})");

        // Calculate backtrack path
        List<string> newBacktrackPath = new List<string>();

        // If we need to backtrack (current location is beyond the common point)
        if (netProgressRoute.Count > lastCommonIndex + 1)
        {
            // We need to backtrack from current location to the common point
            // Add all points that need to be "undone" in the order we'll visit them
            for (int i = netProgressRoute.Count - 1; i > lastCommonIndex; i--)
            {
                newBacktrackPath.Add(netProgressRoute[i]);
            }
        }

        Debug.Log($"Points to backtrack through: {string.Join("->", newBacktrackPath)}");

        // Build the complete journey plan
        driverJourneyPlanIDs.Clear();

        // Start from current location (don't duplicate it in the plan)
        driverJourneyPlanIDs.Add(currentLocation);

        // Add backtrack path (exclude current location since we already added it)
        for (int i = 0; i < newBacktrackPath.Count; i++)
        {
            if (newBacktrackPath[i] != currentLocation)
            {
                driverJourneyPlanIDs.Add(newBacktrackPath[i]);
            }
        }

        // Add forward path from the point after common point
        for (int i = lastCommonIndex + 1; i < liveRoute.Count; i++)
        {
            // Avoid duplicating the common point if we're already there
            if (driverJourneyPlanIDs.Count == 0 || driverJourneyPlanIDs.Last() != liveRoute[i])
            {
                driverJourneyPlanIDs.Add(liveRoute[i]);
            }
        }

        // Set backtracking state
        isBacktracking = newBacktrackPath.Count > 0;
        backtrackStepsRemaining = newBacktrackPath.Count;
        backtrackPath = new List<string>(newBacktrackPath);

        Debug.Log($"Geri dönüþ yolu: {string.Join("->", newBacktrackPath)}");
        Debug.Log($"NÝHAÝ YOLCULUK PLANI: {string.Join("->", driverJourneyPlanIDs)}");
        Debug.Log($"Geri Dönüþ Aktif mi? {isBacktracking}, Adým Sayýsý: {backtrackStepsRemaining}");
        Debug.Log("------------------------------------");

        RebuildAndActivateRoute();
    }

    public void PlayerReachedPoint(string pointID)
    {
        Debug.Log($"REACHED: {pointID}");

        // Always add to total travel log
        totalTravelLogRoute.Add(pointID);

        // Handle backtracking mode
        if (isBacktracking && backtrackStepsRemaining > 0)
        {
            Debug.Log($"Backtracking mode: Expecting backtrack point. Remaining steps: {backtrackStepsRemaining}");
            Debug.Log($"Backtrack path: {string.Join("->", backtrackPath)}");

            // During backtracking, we should remove points from netProgressRoute as we visit them going backwards
            // The point we just reached should be removed from the END of netProgressRoute
            if (netProgressRoute.Count > 0 && netProgressRoute.Last() == pointID)
            {
                netProgressRoute.RemoveAt(netProgressRoute.Count - 1);
                Debug.Log($"BACKTRACK: {pointID} noktasý NetProgressRoute'tan çýkarýldý.");
                Debug.Log($"NetProgressRoute: {string.Join("->", netProgressRoute)}");

                backtrackStepsRemaining--;
                Debug.Log($"Kalan geri dönüþ adýmý: {backtrackStepsRemaining}");

                if (backtrackStepsRemaining == 0)
                {
                    isBacktracking = false;
                    backtrackPath.Clear();
                    Debug.Log("Geri dönüþ tamamlandý. Artýk ileri doðru hareket edilebilir.");
                }
            }
            else
            {
                Debug.LogWarning($"BACKTRACK ERROR: Expected to remove {pointID} from end of NetProgressRoute, but last point is {(netProgressRoute.Count > 0 ? netProgressRoute.Last() : "none")}");
            }
            return;
        }

        // Handle forward progress mode
        if (!isBacktracking)
        {
            Debug.Log($"Forward progress mode. Current NetProgressRoute: {string.Join("->", netProgressRoute)}");
            Debug.Log($"Current LiveRoute: {string.Join("->", liveRoute)}");

            // Check if this point should be added to netProgressRoute
            if (!netProgressRoute.Contains(pointID))
            {
                // Find where this point should be in the liveRoute
                int pointIndexInLiveRoute = liveRoute.IndexOf(pointID);

                if (pointIndexInLiveRoute == -1)
                {
                    Debug.LogWarning($"Point {pointID} is not in current LiveRoute!");
                    return;
                }

                // Check if this is the next expected point in sequence
                int expectedIndex = netProgressRoute.Count;

                if (pointIndexInLiveRoute == expectedIndex)
                {
                    netProgressRoute.Add(pointID);
                    Debug.Log($"PROGRESS: {pointID} noktasý NetProgressRoute'a eklendi.");
                    Debug.Log($"NetProgressRoute: {string.Join("->", netProgressRoute)}");

                    // Check if route is completed
                    if (netProgressRoute.Count == liveRoute.Count)
                    {
                        RouteFinished = true;
                        Debug.Log("Rota tamamlandý!");
                    }
                }
                else
                {
                    Debug.LogWarning($"OUT OF SEQUENCE: {pointID} reached at wrong time. Expected index: {expectedIndex}, actual index in LiveRoute: {pointIndexInLiveRoute}");
                    Debug.LogWarning($"Expected point: {(expectedIndex < liveRoute.Count ? liveRoute[expectedIndex] : "route complete")}");
                }
            }
            else
            {
                Debug.LogWarning($"Point {pointID} already exists in NetProgressRoute - this shouldn't happen in forward progress!");
            }
        }
    }

    private void RebuildAndActivateRoute()
    {
        ResetRoute(false);

        if (!TryBuildRouteFromIDs(driverJourneyPlanIDs, out driverJourneyPoints, out driverJourneySplines))
        {
            Debug.LogError("ÞOFÖR YOLCULUK PLANI KURULAMADI.", this.gameObject);
            RouteInitialized = false;
            return;
        }

        CalculateTotalRouteLength();
        DeactivateAllSplinesInScene();

        RouteInitialized = true;
        Debug.Log($"Yeni Yolculuk Planý Fiziksel Olarak Aktif Edildi.");
        ActivateNextSplines();
    }

    private void InitializeRoute()
    {
        ResetRoute(true);
        SetupInitialRouteLists();
        driverJourneyPlanIDs = new List<string>(liveRoute);
        RebuildAndActivateRoute();

        CheckAndHandleInitialOverlap();
    }

    #region Unchanged Code (No modifications below this line)

    private void CheckAndHandleInitialOverlap()
    {
        if (playerCollider == null || driverJourneyPoints.Count == 0) return;

        if (netProgressRoute.Count == 0)
        {
            ISPoint firstPoint = driverJourneyPoints[0];
            if (firstPoint.TryGetComponent<Collider>(out var firstPointTrigger))
            {
                if (playerCollider.bounds.Intersects(firstPointTrigger.bounds))
                {
                    PlayerReachedPoint(firstPoint.IntersectionID);
                }
            }
        }
    }

    private ISSpline FindSplineBetween(ISPoint start, ISPoint end)
    {
        foreach (var splineGO in start.OutgoingSplines)
        {
            if (splineGO.TryGetComponent<ISSpline>(out var splineComponent) && splineComponent.EndIntersection == end)
                return splineComponent;
        }
        foreach (var splineGO in start.IncomingSplines)
        {
            if (splineGO.TryGetComponent<ISSpline>(out var splineComponent) && splineComponent.StartIntersection == end)
                return splineComponent;
        }
        return null;
    }

    private bool TryBuildPointMap(out Dictionary<string, ISPoint> pointMap)
    {
        pointMap = new Dictionary<string, ISPoint>();
        foreach (var point in FindObjectsByType<ISPoint>(FindObjectsSortMode.None))
        {
            point.TheRouteManager = this;
            if (string.IsNullOrEmpty(point.IntersectionID)) continue;
            if (pointMap.ContainsKey(point.IntersectionID))
            {
                Debug.LogError($"Tekrarlanan IntersectionID hatasý! ID: '{point.IntersectionID}'", point.gameObject);
                return false;
            }
            pointMap.Add(point.IntersectionID, point);
        }
        return true;
    }

    private bool TryBuildRouteFromIDs(List<string> routeIDs, out List<ISPoint> points, out List<ISSpline> splines)
    {
        points = new List<ISPoint>();
        splines = new List<ISSpline>();

        if (!TryBuildPointMap(out var allPointsInScene))
        {
            return false;
        }

        foreach (var pointID in routeIDs)
        {
            if (allPointsInScene.TryGetValue(pointID, out ISPoint point))
            {
                points.Add(point);
            }
            else
            {
                Debug.LogWarning($"'{pointID}' ID'li bir ISPoint sahnede bulunamadý.", this.gameObject);
                return false;
            }
        }

        for (int i = 0; i < points.Count - 1; i++)
        {
            ISSpline connectingSpline = FindSplineBetween(points[i], points[i + 1]);
            if (connectingSpline != null)
            {
                splines.Add(connectingSpline);
            }
            else
            {
                Debug.LogWarning($"'{points[i].IntersectionID}' ve '{points[i + 1].IntersectionID}' arasýnda HÝÇBÝR YÖNDE spline bulunamadý.");
                return false;
            }
        }
        return true;
    }

    public (float length, int pointCount) PreviewAndCalculateRouteStats()
    {
        float calculatedLength = 0f;
        if (!TryBuildPointMap(out var allPointsInScene)) { return (0, 0); }
        for (int i = 0; i < initialRoute.Count - 1; i++)
        {
            ISPoint startPoint = allPointsInScene.GetValueOrDefault(initialRoute[i]);
            ISPoint endPoint = allPointsInScene.GetValueOrDefault(initialRoute[i + 1]);
            if (startPoint == null || endPoint == null) { return (0, 0); }
            ISSpline connectingSpline = FindSplineBetween(startPoint, endPoint);
            if (connectingSpline != null) { calculatedLength += connectingSpline.Length; }
            else { return (0, 0); }
        }
        return (calculatedLength, initialRoute.Count);
    }

    private void SetupInitialRouteLists()
    {
        liveRoute = new List<string>(initialRoute);
        netProgressRoute.Clear();
        totalTravelLogRoute.Clear();
    }

    private void ResetRoute(bool resetAllLogs)
    {
        if (resetAllLogs)
        {
            routeTimerManager?.ResetTimer();
            netProgressRoute.Clear();
            totalTravelLogRoute.Clear();
        }

        foreach (var spline in driverJourneySplines)
        {
            if (spline != null && spline.gameObject.activeSelf)
            {
                spline.gameObject.SetActive(false);
            }
        }

        driverJourneyPoints.Clear();
        driverJourneySplines.Clear();
        CurrentJourneyStepIndex = 0;
        TotalRouteLength = 0f;
        TotalDistanceTraveled = 0f;
        distanceTraveledOnPreviousSplines = 0f;
        ClosestPointOnRoute = Vector3.zero;
        RouteInitialized = false;
        RouteFinished = false;

        // Don't reset backtracking state here as it should persist during route rebuilds
    }

    public void UpdatePlayerProgress(Vector3 closestPoint, float tValueOnCurrentSpline, ISSpline currentSpline)
    {
        if (currentSpline == null) return;
        this.ClosestPointOnRoute = closestPoint;
        float clampedT = Mathf.Clamp01(tValueOnCurrentSpline);
        float distanceOnCurrentSpline = currentSpline.Length * clampedT;
        this.TotalDistanceTraveled = distanceTraveledOnPreviousSplines + distanceOnCurrentSpline;
    }

    private void CalculateTotalRouteLength()
    {
        TotalRouteLength = 0;
        foreach (var spline in driverJourneySplines)
        {
            if (spline != null) TotalRouteLength += spline.Length;
        }
    }

    private void DeactivateAllSplinesInScene()
    {
        var allSplinesInScene = FindObjectsByType<ISSpline>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var spline in allSplinesInScene)
        {
            spline.gameObject.SetActive(false);
        }
    }

    private void ActivateNextSplines()
    {
        if (CurrentJourneyStepIndex > 0)
        {
            GameObject previousSplineObject = driverJourneySplines[CurrentJourneyStepIndex - 1].gameObject;
            StartCoroutine(DisableAfterFrame(previousSplineObject));
        }
        if (CurrentJourneyStepIndex < driverJourneySplines.Count)
        {
            driverJourneySplines[CurrentJourneyStepIndex].gameObject.SetActive(true);
        }
        if (CurrentJourneyStepIndex + 1 < driverJourneySplines.Count)
        {
            driverJourneySplines[CurrentJourneyStepIndex + 1].gameObject.SetActive(true);
        }
    }

    public ISSpline GetCurrentActiveSpline()
    {
        if (RouteInitialized && !RouteFinished && CurrentJourneyStepIndex < driverJourneySplines.Count)
        {
            return driverJourneySplines[CurrentJourneyStepIndex];
        }
        return null;
    }

    private System.Collections.IEnumerator DisableAfterFrame(GameObject obj)
    {
        yield return null;
        if (obj != null) obj.SetActive(false);
    }

    public bool TryGetPointAndDirectionAtDistance(float targetDistance, out Vector3 worldPoint, out Vector3 worldForwardDirection)
    {
        worldPoint = Vector3.zero;
        worldForwardDirection = Vector3.forward;
        if (!RouteInitialized || targetDistance < 0 || targetDistance > this.TotalRouteLength) return false;
        if (Mathf.Approximately(targetDistance, TotalRouteLength))
        {
            var lastSpline = driverJourneySplines.Last();
            var container = lastSpline.GetComponent<SplineContainer>();
            worldPoint = container.transform.TransformPoint(container.Spline.EvaluatePosition(1f));
            worldForwardDirection = Vector3.Normalize(container.transform.TransformDirection(container.Spline.EvaluateTangent(1f)));
            return true;
        }
        float cumulativeLength = 0f;
        foreach (var spline in driverJourneySplines)
        {
            float splineLength = spline.Length;
            if (targetDistance <= cumulativeLength + splineLength)
            {
                var container = spline.GetComponent<SplineContainer>();
                if (container == null) return false;
                float distanceIntoSpline = targetDistance - cumulativeLength;
                float t = (splineLength > 0) ? distanceIntoSpline / splineLength : 0;
                worldPoint = container.transform.TransformPoint(container.Spline.EvaluatePosition(t));
                worldForwardDirection = Vector3.Normalize(container.transform.TransformDirection(container.Spline.EvaluateTangent(t)));
                return true;
            }
            cumulativeLength += splineLength;
        }
        return false;
    }

    #endregion
}

public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : UnityEditor.PropertyDrawer
{
    public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        UnityEditor.EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif

*/