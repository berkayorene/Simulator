
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class RouteManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private RouteTimerManager routeTimerManager;
    [Tooltip("Konumu takip edilecek olan şoförün (otobüsün) Collider'ı.")]
    [SerializeField] private Collider playerCollider;

    [Header("Baking Target (Editor Only)")]
    [SerializeField] private LevelData levelDataToBake;
    public LevelData LevelDataToBake => levelDataToBake;

    [Header("Route Definitions")]
    [Tooltip("Seviyenin en başında belirlenen, asla değişmeyen orijinal rota.")]
    [SerializeField] private List<string> initialRoute;
    public IReadOnlyList<string> InitialRoute => initialRoute;
    public List<string> RoutePointIDs => initialRoute;

    [Tooltip("Admin tarafından belirlenen, rotanın OLMASI GEREKEN NİHAİ halidir.")]
    [field: SerializeField, ReadOnly]
    private List<string> liveRoute = new List<string>();
    public IReadOnlyList<string> LiveRoute => liveRoute;

    [Tooltip("Şoförün geri dönüşler hariç net ilerlemesini gösteren rota.")]
    [field: SerializeField, ReadOnly]
    private List<string> netProgressRoute = new List<string>();
    public IReadOnlyList<string> NetProgressRoute => netProgressRoute;

    [Tooltip("Şoförün geçtiği her bir noktanın tam seyahat kaydı.")]
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
            Debug.LogError("Player Collider atanmamış! Başlangıç noktası kontrolü yapılamaz.", this);
        }

        if (initialRoute != null && initialRoute.Count > 1)
        {
            // InitializeRoute metodunu direkt çağırmak yerine Coroutine olarak başlatıyoruz.
            StartCoroutine(InitializeRoute());
        }
        else
        {
            Debug.LogWarning("RouteManager'da 'Initial Route' tanımlanmamış. Rota kurulamıyor.", this);
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
            Debug.LogError($"Geçersiz rota! Rota uygulanamadı: {string.Join("->", newAdminRoute)}");
            return;
        }

        // Check if starting point is the same
        if (liveRoute.Count > 0 && newAdminRoute[0] != liveRoute[0])
        {
            Debug.LogError($"Başlangıç noktası değiştirilemiyor! Eski: {liveRoute[0]}, Yeni: {newAdminRoute[0]}");
            return;
        }

        this.liveRoute = new List<string>(newAdminRoute);
        Debug.Log($"Admin Emri Alındı: Yeni LiveRoute -> {string.Join("->", liveRoute)}");

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
                Debug.LogError($"'{pointID}' ID'li ISPoint sahnede bulunamadı!");
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
                Debug.LogError($"'{routeToValidate[i]}' ve '{routeToValidate[i + 1]}' arasında bağlantı bulunamadı!");
                return false;
            }
        }

        return true;
    }

    private void CalculateAndApplyNewJourneyPlan()
    {
        if (netProgressRoute.Count == 0)
        {
            Debug.Log("NetProgressRoute boş, ilk rota olarak ayarlanıyor.");
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
            Debug.LogError("Mevcut konum bulunamadı, yeni plan hesaplanamıyor.");
            return;
        }

        Debug.Log("--- Yeni Yolculuk Planı Hesaplanıyor ---");
        Debug.Log($"Mevcut Konum: {currentLocation}");
        Debug.Log($"Net İlerleme Yolu (NPR): {string.Join("->", netProgressRoute)}");
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
            Debug.LogError("Yeni rota ile ortak nokta bulunamadı!");
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

        Debug.Log($"Geri dönüş yolu: {string.Join("->", newBacktrackPath)}");
        Debug.Log($"NİHAİ YOLCULUK PLANI: {string.Join("->", driverJourneyPlanIDs)}");
        Debug.Log($"Geri Dönüş Aktif mi? {isBacktracking}, Adım Sayısı: {backtrackStepsRemaining}");
        Debug.Log("------------------------------------");

        RebuildAndActivateRoute();
    }

    public void PlayerReachedPoint(string pointID)
    {

        if (totalTravelLogRoute.Count > 0 && totalTravelLogRoute.LastOrDefault() == pointID)
        {
            return; // Metottan hemen çık
        }

        Debug.Log($"REACHED: {pointID}");

        routeTimerManager?.LogCheckpointTime(pointID);

        // Her zaman toplam seyahat kaydına ekle
        totalTravelLogRoute.Add(pointID);

        // Geri geri gitme (backtracking) modunu işle
        if (isBacktracking && backtrackStepsRemaining > 0)
        {
            Debug.Log($"Backtracking mode: Expecting backtrack point. Remaining steps: {backtrackStepsRemaining}");
            Debug.Log($"Backtrack path: {string.Join("->", backtrackPath)}");

            if (netProgressRoute.Count > 0 && netProgressRoute.Last() == pointID)
            {
                netProgressRoute.RemoveAt(netProgressRoute.Count - 1);
                Debug.Log($"BACKTRACK: {pointID} noktası NetProgressRoute'tan çıkarıldı.");
                Debug.Log($"NetProgressRoute: {string.Join("->", netProgressRoute)}");

                backtrackStepsRemaining--;
                Debug.Log($"Kalan geri dönüş adımı: {backtrackStepsRemaining}");

                if (backtrackStepsRemaining == 0)
                {
                    isBacktracking = false;
                    backtrackPath.Clear();
                    Debug.Log("Geri dönüş tamamlandı. Artık ileri doğru hareket edilebilir.");
                }
            }
            else
            {
                Debug.LogWarning($"BACKTRACK ERROR: Expected to remove {pointID} from end of NetProgressRoute, but last point is {(netProgressRoute.Count > 0 ? netProgressRoute.Last() : "none")}");
            }

            // YENİ: Geri giderken de spline'ları güncellemeliyiz.
            int reachedPointIndexInJourney = driverJourneyPlanIDs.IndexOf(pointID);
            if (reachedPointIndexInJourney != -1)
            {
                CurrentJourneyStepIndex = reachedPointIndexInJourney - 1; // Bir önceki spline'a odaklan
                if (CurrentJourneyStepIndex < 0) CurrentJourneyStepIndex = 0;
                ActivateNextSplines();
            }

            return;
        }

        // İleri doğru ilerleme modunu işle
        if (!isBacktracking)
        {
            Debug.Log($"Forward progress mode. Current NetProgressRoute: {string.Join("->", netProgressRoute)}");
            Debug.Log($"Current LiveRoute: {string.Join("->", liveRoute)}");

            if (!netProgressRoute.Contains(pointID))
            {
                int pointIndexInLiveRoute = liveRoute.IndexOf(pointID);

                if (pointIndexInLiveRoute == -1)
                {
                    Debug.LogWarning($"Point {pointID} is not in current LiveRoute!");
                    return;
                }

                int expectedIndex = netProgressRoute.Count;

                if (pointIndexInLiveRoute == expectedIndex)
                {
                    netProgressRoute.Add(pointID);
                    Debug.Log($"PROGRESS: {pointID} noktası NetProgressRoute'a eklendi.");
                    Debug.Log($"NetProgressRoute: {string.Join("->", netProgressRoute)}");

                    // --- YENİ EKLENEN KOD BAŞLANGICI ---
                    // Ulaşılan noktanın, şoförün takip ettiği ana yolculuk planındaki (driverJourneyPlanIDs)
                    // indeksini buluyoruz. Bu indeks, bir sonraki aktif spline'ı belirlemek için kullanılacak.
                    int reachedPointIndexInJourney = driverJourneyPlanIDs.IndexOf(pointID);
                    if (reachedPointIndexInJourney != -1)
                    {
                        // Ulaşılan nokta, bir önceki spline'ın tamamlandığı anlamına gelir.
                        // Tamamlanan spline'ın indeksini bulalım (ulaşılan noktanın indeksinden bir eksik).
                        int completedSplineIndex = reachedPointIndexInJourney - 1;

                        // Bu indeksin geçerli bir spline'a karşılık geldiğinden emin olalım.
                        // (Bu kontrol, rotanın en başındaki ilk nokta için hatayı önler, çünkü onun öncesinde spline yoktur).
                        if (completedSplineIndex >= 0 && completedSplineIndex < driverJourneySplines.Count)
                        {
                            ISSpline completedSpline = driverJourneySplines[completedSplineIndex];
                            // Tamamlanan spline'ın uzunluğunu kümülatif mesafeye ekliyoruz.
                            distanceTraveledOnPreviousSplines += completedSpline.Length;
                            Debug.Log($"Spline '{completedSpline.SplineID}' tamamlandı. Kümülatif mesafeye {completedSpline.Length} eklendi. Yeni toplam: {distanceTraveledOnPreviousSplines}");
                        }


                        // Mevcut adım indeksini ulaştığımız noktanın indeksi olarak ayarlıyoruz.
                        // Örneğin, A-B-C rotasında B'ye ulaştıysak (indeks 1), artık B->C spline'ı (indeks 1)
                        // bizim "mevcut" spline'ımız olur.
                        CurrentJourneyStepIndex = reachedPointIndexInJourney;

                        // Görsel olarak bir sonraki spline'ları aktif etmesi için metodu çağırıyoruz.
                        ActivateNextSplines();
                        Debug.Log($"Spline'lar güncellendi. Yeni CurrentJourneyStepIndex: {CurrentJourneyStepIndex}");
                    }
                    else
                    {
                        Debug.LogWarning($"Ulaşılan nokta ({pointID}) mevcut yolculuk planında bulunamadı!");
                    }
                    // --- YENİ EKLENEN KOD SONU ---


                    // Rota tamamlandı mı kontrol et
                    if (netProgressRoute.Count == liveRoute.Count)
                    {
                        RouteFinished = true;
                        TotalDistanceTraveled = TotalRouteLength;
                        routeTimerManager?.StopTimer();

                        RunSummaryData summary = RunSummaryGenerator.Generate(this, routeTimerManager);

                        PrintSummaryToConsole(summary);

                        GameSessionManager.Instance.LastRunSummary = summary;

                        Debug.Log("Rota tamamlandı!");


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
            Debug.LogError("ŞOFÖR YOLCULUK PLANI KURULAMADI.", this.gameObject);
            RouteInitialized = false;
            return;
        }

        CalculateTotalRouteLength();
        DeactivateAllSplinesInScene();

        RouteInitialized = true;
        Debug.Log($"Yeni Yolculuk Planı Fiziksel Olarak Aktif Edildi.");
        ActivateNextSplines();
    }


    private IEnumerator InitializeRoute()
    {
        // Rota listelerini ve durumunu sıfırla
        ResetRoute(true);
        SetupInitialRouteLists();
        driverJourneyPlanIDs = new List<string>(liveRoute);

        // Rotayı ID'lere göre kur ve objeleri (point, spline) referans al
        if (!TryBuildRouteFromIDs(driverJourneyPlanIDs, out driverJourneyPoints, out driverJourneySplines))
        {
            Debug.LogError("İLK ROTA KURULAMADI (INITIALIZE).", this.gameObject);
            RouteInitialized = false;
            yield break; // Coroutine'i sonlandır
        }

        // Rota ile ilgili hesaplamaları yap
        CalculateTotalRouteLength();
        DeactivateAllSplinesInScene();
        RouteInitialized = true;
        Debug.Log($"İlk Rota Kuruldu ve Aktif Edilmeye Hazır.");

        // --- İSTEĞİN ÜZERİNE EKLENEN 1 SANİYELİK BEKLEME ---
        Debug.Log("Oyun başlangıcı: Spline aktivasyonu için 1 saniye bekleniyor...");
        yield return new WaitForSeconds(1f);
        Debug.Log("Bekleme tamamlandı. İlk spline'lar şimdi aktive ediliyor.");
        // --- BEKLEME SONU ---

        // Bekleme sonrası ilk spline'ları görünür yap
        ActivateNextSplines();

        // Başlangıç noktasında olup olmadığımızı kontrol et
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
                Debug.LogError($"Tekrarlanan IntersectionID hatası! ID: '{point.IntersectionID}'", point.gameObject);
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
                Debug.LogWarning($"'{pointID}' ID'li bir ISPoint sahnede bulunamadı.", this.gameObject);
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
                Debug.LogWarning($"'{points[i].IntersectionID}' ve '{points[i + 1].IntersectionID}' arasında HİÇBİR YÖNDE spline bulunamadı.");
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

    /// <summary>
    /// Verilen RunSummaryData nesnesinin içeriğini Unity konsoluna detaylı bir şekilde yazdırır.
    /// Bu metot, test ve hata ayıklama için kullanılır.
    /// </summary>
    /// <param name="data">İçeriği yazdırılacak olan özet verisi.</param>
    private void PrintSummaryToConsole(RunSummaryData data)
    {
        if (data == null)
        {
            Debug.LogWarning("Özet verisi (RunSummaryData) boş, rapor yazılamadı.");
            return;
        }

        Debug.Log("--- --- --- OYUN SONU ÖZET RAPORU --- --- ---");

        // Genel Bilgiler
        Debug.Log($"Tamamlanma Zamanı: {data.CompletionTimestamp}");
        Debug.Log($"Rota Başarıyla Bitirildi mi?: {data.WasRouteFinished}");

        // Performans Metrikleri
        Debug.Log("--- Performans ---");
        Debug.Log($"Toplam Geçen Süre: {data.TotalElapsedTime:F2} saniye");
        Debug.Log($"Sürüş Verimliliği: %{data.EfficiencyPercentage:F1}");
        Debug.Log($"Ortalama Hız: {data.AverageSpeedKmh:F1} km/s");

        // Rota ve Mesafe Detayları
        Debug.Log("--- Rota ve Mesafe Detayları ---");
        Debug.Log($"Initial Route ({data.InitialRouteLength:F1}m): {string.Join(" -> ", data.InitialRoute)}");
        Debug.Log($"Final Live Route ({data.FinalLiveRouteLength:F1}m): {string.Join(" -> ", data.FinalLiveRoute)}");
        Debug.Log($"Net Progress Route ({data.NetProgressRouteLength:F1}m): {string.Join(" -> ", data.NetProgressRoute)}");
        Debug.Log($"Total Travel Log ({data.TotalTravelLogLength:F1}m): {string.Join(" -> ", data.TotalTravelLog)}");

        // Checkpoint Zamanları
        Debug.Log("--- Checkpoint Varış Süreleri ---");
        if (data.CheckpointTimes != null && data.CheckpointTimes.Count > 0)
        {
            foreach (var checkpoint in data.CheckpointTimes)
            {
                Debug.Log($"Nokta: {checkpoint.Key} -> {checkpoint.Value:F2}. saniyede ulaşıldı.");
            }
        }
        else
        {
            Debug.Log("Kaydedilmiş checkpoint zamanı yok.");
        }

        // Segment Hızları
        Debug.Log("--- Segment Hızları (km/s) ---");
        if (data.SegmentSpeedsKmh != null && data.SegmentSpeedsKmh.Count > 0)
        {
            foreach (var segment in data.SegmentSpeedsKmh)
            {
                Debug.Log($"Segment: {segment.Key} -> Ortalama Hız: {segment.Value:F1} km/s");
            }
        }
        else
        {
            Debug.Log("Hesaplanmış segment hızı yok.");
        }

        Debug.Log("--- --- --- RAPOR SONU --- --- ---");
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
