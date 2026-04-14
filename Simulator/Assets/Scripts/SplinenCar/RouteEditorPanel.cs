using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class RouteEditorPanel : MonoBehaviour
{
    [Header("Ana Referanslar")]
    [SerializeField] private RouteManager routeManager;
    [SerializeField] private GameObject editorCanvas;

    [Header("UI Elemanlarý")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button undoButton;
    [SerializeField] private Transform draftRouteListContainer;
    [SerializeField] private GameObject routeStepTextPrefab;
    // YENÝ: Uyarý metni için referans ve ayarlar eklendi.
    [Tooltip("Geçersiz rota gibi uyarýlarý göstermek için kullanýlacak TextMeshPro elemaný.")]
    [SerializeField] private TextMeshProUGUI warningText;
    [Tooltip("Uyarý metninin ekranda kalma süresi (saniye).")]
    [SerializeField] private float warningDisplayDuration = 3f;


    // --- Durum Yönetimi ---
    private List<string> draftRoute;
    private Stack<List<string>> routeHistoryStack = new Stack<List<string>>();
    private bool isEditModeActive = false;
    private int editingFromIndex = -1;

    // --- Harita Veri Yönetimi ---
    private Dictionary<string, ISPointMarker> allMarkers = new Dictionary<string, ISPointMarker>();
    private Dictionary<string, ISPoint> allMapPoints = new Dictionary<string, ISPoint>();


    private void Awake()
    {
        var points = FindObjectsByType<ISPoint>(FindObjectsSortMode.None);
        foreach (var point in points)
        {
            if (!string.IsNullOrEmpty(point.IntersectionID))
                allMapPoints[point.IntersectionID] = point;
        }

        var markers = FindObjectsByType<ISPointMarker>(FindObjectsSortMode.None);
        foreach (var marker in markers)
        {
            if (!string.IsNullOrEmpty(marker.PointID))
            {
                allMarkers[marker.PointID] = marker;
                marker.OnStepClicked += OnStepMarkerClicked;
                marker.OnSelectClicked += OnSelectNeighborMarker;
                marker.OnDeleteClicked += OnDeleteMarker;
            }
        }
    }

    private void Start()
    {
        editorCanvas.SetActive(true);
        confirmButton.onClick.AddListener(ConfirmChanges);
        cancelButton.onClick.AddListener(CancelChanges);
        undoButton.onClick.AddListener(UndoLastChange);

        // YENÝ: Baþlangýçta uyarý metnini gizle.
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
        }

        Invoke(nameof(StartEditSession), 0.1f);
    }

    private void StartEditSession()
    {
        if (routeManager == null || !routeManager.RouteInitialized)
        {
            Debug.LogError("RouteManager hazýr deðil! Editör seansý baþlatýlamadý.");
            return;
        }

        draftRoute = new List<string>(routeManager.LiveRoute);
        routeHistoryStack.Clear();
        routeHistoryStack.Push(new List<string>(draftRoute));
        editingFromIndex = -1;

        UpdateAllUI();
    }

    private void UpdateAllUI()
    {
        UpdateMarkersUI();
        UpdateRouteListUI();
    }

    private void UpdateMarkersUI()
    {
        foreach (var marker in allMarkers.Values)
        {
            marker.ResetState();
        }

        for (int i = 0; i < draftRoute.Count; i++)
        {
            if (allMarkers.TryGetValue(draftRoute[i], out ISPointMarker marker))
            {
                marker.SetAsRouteStep(i);
                marker.ShowDeleteButton();
            }
        }

        if (editingFromIndex != -1)
        {
            string editStartPointID = draftRoute[editingFromIndex];
            if (allMapPoints.TryGetValue(editStartPointID, out ISPoint editStartPoint3D))
            {
                foreach (var splineGO in editStartPoint3D.OutgoingSplines)
                {
                    if (splineGO.TryGetComponent<ISSpline>(out var spline))
                    {
                        if (allMarkers.TryGetValue(spline.EndIntersection.IntersectionID, out ISPointMarker neighborMarker))
                        {
                            neighborMarker.ShowSelectButton();
                        }
                    }
                }
            }
        }
    }

    private void UpdateRouteListUI()
    {
        foreach (Transform child in draftRouteListContainer)
        {
            Destroy(child.gameObject);
        }

        if (draftRoute == null || draftRoute.Count == 0 || routeStepTextPrefab == null) return;

        for (int i = 0; i < draftRoute.Count; i++)
        {
            GameObject textInstance = Instantiate(routeStepTextPrefab, draftRouteListContainer);
            var textComponent = textInstance.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = $"{i + 1}. {draftRoute[i]}";
            }
        }
    }


    // --- Olay Dinleyicileri (Event Handlers) ---

    private void OnStepMarkerClicked(int routeIndex)
    {
        editingFromIndex = routeIndex;
        UpdateAllUI();
    }

    private void OnSelectNeighborMarker(string pointID)
    {
        draftRoute.Insert(editingFromIndex + 1, pointID);
        editingFromIndex++;

        routeHistoryStack.Push(new List<string>(draftRoute));
        UpdateAllUI();
    }

    private void OnDeleteMarker(string pointID, int routeIndex)
    {
        editingFromIndex = -1;
        draftRoute.RemoveAt(routeIndex);

        routeHistoryStack.Push(new List<string>(draftRoute));
        UpdateAllUI();
    }

    // --- Ana Kontrol Butonlarý ---

    private void ConfirmChanges()
    {
        if (!IsRouteValid(draftRoute))
        {
            // DEÐÝÞTÝ: Konsola log atmak yerine UI'da uyarý göster.
            ShowWarning("Girdiðiniz rota geçerli deðil!");
            return;
        }

        if (!IsProgressCompatible(draftRoute))
        {
            ShowWarning("Bu rota geçilen yola uymuyor, sadece ileride olan yollarý deðiþtirebilirsiniz!");
            return; // Uyumlu deðilse iþlemi sonlandýr.
        }


        Debug.Log($"Deðiþiklikler Onaylandý! Final Rota: {string.Join("->", draftRoute)}");
        routeManager.UpdateLiveRoute(draftRoute);
        StartEditSession();
    }

    private void CancelChanges()
    {
        // RouteManager'dan mevcut canlý rotayý alarak düzenlemeleri sýfýrla.
        if (routeManager != null && routeManager.RouteInitialized)
        {
            // Taslak rotayý, mevcut canlý rotanýn bir kopyasýyla deðiþtir.
            draftRoute = new List<string>(routeManager.LiveRoute);

            // Rota geçmiþini temizle ve sýfýrlanmýþ bu hali ilk durum olarak ekle.
            routeHistoryStack.Clear();
            routeHistoryStack.Push(new List<string>(draftRoute));

            // Düzenleme modundan çýk.
            editingFromIndex = -1;

            // UI'ý bu sýfýrlanmýþ duruma göre güncelle.
            UpdateAllUI();

            Debug.Log("Deðiþiklikler iptal edildi. Rota, mevcut canlý rotaya geri döndürüldü.");
        }
        else
        {
            Debug.LogWarning("Deðiþiklikler iptal edilemedi, RouteManager hazýr deðil.");
        }
    }
    private void UndoLastChange()
    {
        // Yýðýnda mevcut durumdan baþka en az bir geçmiþ durum varsa...
        if (routeHistoryStack.Count > 1)
        {
            // Mevcut (hatalý/istenmeyen) durumu yýðýndan at.
            routeHistoryStack.Pop();

            // Yýðýnýn en üstündeki yeni duruma (bir önceki durum) geri dön.
            // Peek() metodu, yýðýndan çýkarmadan en üstteki elemaný okur.
            draftRoute = new List<string>(routeHistoryStack.Peek());

            // Geri alma iþleminden sonra hangi noktadan düzenleme yapýlacaðý belirsizleþir, bu yüzden sýfýrla.
            editingFromIndex = -1;

            // UI'ý bu geri alýnmýþ duruma göre güncelle.
            UpdateAllUI();

            Debug.Log("Son deðiþiklik geri alýndý.");
        }
        else
        {
            Debug.Log("Geri alýnacak bir deðiþiklik bulunmuyor.");
        }
    }
    private bool IsRouteValid(List<string> route)
    {
        if (route.Count < 2) return true;

        for (int i = 0; i < route.Count - 1; i++)
        {
            string currentPointID = route[i];
            string nextPointID = route[i + 1];

            if (allMapPoints.TryGetValue(currentPointID, out ISPoint currentPoint3D))
            {
                bool connectionFound = false;
                foreach (var splineGO in currentPoint3D.OutgoingSplines)
                {
                    if (splineGO.TryGetComponent<ISSpline>(out var spline) && spline.EndIntersection.IntersectionID == nextPointID)
                    {
                        connectionFound = true;
                        break;
                    }
                }
                if (!connectionFound) return false;
            }
            else return false;
        }
        return true;
    }

    /// <summary>
    /// Yeni rotanýn (draftRoute), oyuncunun mevcut ilerlemesiyle (NetProgressRoute) uyumlu olup olmadýðýný kontrol eder.
    /// Uyumlu olmasý için, yeni rotanýn oyuncunun katettiði yolu baþtan itibaren içermesi gerekir.
    /// </summary>
    /// <param name="newRoute">Kontrol edilecek yeni rota taslaðý.</param>
    /// <returns>Eðer rota uyumluysa true, deðilse false döner.</returns>
    private bool IsProgressCompatible(List<string> newRoute)
    {
        if (routeManager == null) return false;

        // RouteManager'dan oyuncunun katettiði net yolu al.
        IReadOnlyList<string> progressRoute = routeManager.NetProgressRoute;

        // Eðer oyuncu henüz hiç ilerlemediyse (sadece baþlangýç noktasýnda), her rota potansiyel olarak geçerlidir.
        if (progressRoute.Count <= 1)
        {
            return true;
        }

        // Yeni rota, kat edilmiþ yoldan daha kýsa olamaz.
        if (newRoute.Count < progressRoute.Count)
        {
            return false;
        }

        // Yeni rotanýn baþýndaki adýmlarýn, kat edilmiþ yolla birebir ayný olup olmadýðýný kontrol et.
        for (int i = 0; i < progressRoute.Count; i++)
        {
            if (newRoute[i] != progressRoute[i])
            {
                // Herhangi bir adým uyuþmuyorsa, rota uyumsuzdur.
                return false;
            }
        }

        // Tüm kontrollerden geçtiyse, rota oyuncunun ilerlemesiyle uyumludur.
        return true;
    }


    // --- YENÝ: Uyarý gösterme mekanizmasý ---
    private Coroutine activeWarningCoroutine;

    private void ShowWarning(string message)
    {
        if (warningText == null)
        {
            Debug.LogError("Uyarý metni (Warning Text) atanmamýþ!");
            return;
        }

        if (activeWarningCoroutine != null)
        {
            StopCoroutine(activeWarningCoroutine);
        }
        activeWarningCoroutine = StartCoroutine(ShowAndHideWarningRoutine(message));
    }

    private IEnumerator ShowAndHideWarningRoutine(string message)
    {
        warningText.text = message;
        warningText.gameObject.SetActive(true);
        yield return new WaitForSeconds(warningDisplayDuration);
        warningText.gameObject.SetActive(false);
    }
}