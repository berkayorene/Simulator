using UnityEngine;
using System.Collections.Generic;
using System.Linq; 

public class CPManager : MonoBehaviour

{
    [Header("Checkpoint Ayarlarý")]
    [Tooltip("Tüm checkpoint objelerini içeren parent objeyi buraya sürükleyin.")]
    [SerializeField] private Transform checkpointsParent;

    private List<CPInstance> checkpoints;

    [Header("Referanslar")]
    [Tooltip("Oyuncunun Transform'u. Anlýk mesafe takibi için gereklidir.")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("Progress bar'ýn baþlangýç noktasý. Genellikle oyuncunun baþlangýç pozisyonu ile ayný olur.")]
    [SerializeField] private Transform startPoint;

    [SerializeField] private UIManager uiManager;

    // Public properties for the progress bar controller
    public float TotalPathDistance { get; private set; }
    public float CurrentProgressDistance { get; private set; }

    // Eski sayým sistemi (istenirse hala kullanýlabilir)
    public int CurrentArrowCount => currentArrowCount;
    public int MaxArrowCount => checkpoints.Count;

    private int currentArrowCount = 0;
    private float distanceOfCompletedSegments = 0f;

    private void Awake()
    {
        PopulateCheckpointsFromParent();

        if (checkpoints == null || checkpoints.Count == 0 || playerTransform == null || startPoint == null)
        {
            Debug.LogError("CPManager'da Checkpoint listesi, Player Transform veya Start Point atanmamýþ! Lütfen Inspector'dan atamalarý yapýn.");
            return;
        }

        // Checkpoint'lere ID atamasý (eski sistemle uyumluluk için kalabilir)
        for (int i = 0; i < checkpoints.Count; i++)
        {
            checkpoints[i].CpID = i + 1;
        }

        // Toplam yol uzunluðunu hesapla
        CalculateTotalPathDistance();
    }

    private void Update()
    {
        // Oyuncu bir sonraki checkpoint'e doðru ilerlerken mesafeyi hesapla
        UpdateContinuousProgress();
    }

    private void PopulateCheckpointsFromParent()
    {

        checkpoints = new List<CPInstance>();

        if (checkpointsParent == null)
        {
            Debug.LogError("CPManager'da 'Checkpoints Parent' objesi atanmamýþ! Lütfen Inspector'dan atayýn.", this);
            return;
        }

        checkpoints.AddRange(checkpointsParent.GetComponentsInChildren<CPInstance>());

        if (checkpoints.Count == 0)
        {
            Debug.LogWarning("Checkpoints Parent objesinin altýnda hiç aktif CPInstance bulunamadý.", this);
        }
        else
        {
            Debug.Log(checkpoints.Count + " adet checkpoint listeye eklendi.");
        }
    }

    private void CalculateTotalPathDistance()
    {
        TotalPathDistance = 0f;
        if (checkpoints.Count == 0) return;

        // 1. Baþlangýç noktasýndan ilk checkpoint'e olan mesafe
        TotalPathDistance += Vector3.Distance(startPoint.position, checkpoints[0].transform.position);

        // 2. Checkpoint'ler arasý mesafeler
        for (int i = 0; i < checkpoints.Count - 1; i++)
        {
            TotalPathDistance += Vector3.Distance(checkpoints[i].transform.position, checkpoints[i + 1].transform.position);
        }
    }

    private void UpdateContinuousProgress()
    {
        if (currentArrowCount >= checkpoints.Count)
        {
            // Bütün checkpoint'ler toplandýysa, progress %100 olmalý.
            CurrentProgressDistance = TotalPathDistance;
            return;
        }

        // Mevcut segmentin baþlangýç ve bitiþ noktalarýný belirle
        Transform segmentStartTransform = (currentArrowCount == 0) ? startPoint : checkpoints[currentArrowCount - 1].transform;
        Transform segmentEndTransform = checkpoints[currentArrowCount].transform;

        Vector3 segmentStart = segmentStartTransform.position;
        Vector3 segmentEnd = segmentEndTransform.position;

        // Oyuncunun pozisyonunu segment hattý üzerine yansýtarak (project) ilerlemesini bulma.
        // Bu, oyuncu yoldan biraz sapsa bile doðru ilerleme bilgisi verir.
        Vector3 segmentVector = segmentEnd - segmentStart;
        Vector3 playerVector = playerTransform.position - segmentStart;

        // Dot product ile yansýtýlan mesafeyi buluyoruz.
        float progressInSegment = Vector3.Dot(playerVector, segmentVector.normalized);

        // Deðerin 0 ile segmentin toplam uzunluðu arasýnda kalmasýný saðlýyoruz (Clamp).
        progressInSegment = Mathf.Clamp(progressInSegment, 0f, segmentVector.magnitude);

        // Anlýk toplam mesafeyi güncelle
        CurrentProgressDistance = distanceOfCompletedSegments + progressInSegment;
    }


    public void CheckpointHit(CPInstance hitCheckpoint)
    {
        int hitID = hitCheckpoint.CpID;

        if (hitID == currentArrowCount + 1)
        {
            // Önceki segmentin mesafesini hesapla ve ekle
            Transform previousPoint = (currentArrowCount == 0) ? startPoint : checkpoints[currentArrowCount - 1].transform;
            distanceOfCompletedSegments += Vector3.Distance(previousPoint.position, hitCheckpoint.transform.position);

            // Ýlerleme sayacýný artýr
            currentArrowCount++;

            Debug.Log("Doðru Checkpoint: " + hitID + ". Toplam: " + currentArrowCount);

            if (uiManager != null)
            {
                uiManager.ShowCorrectArrowFeedback();
            }

            // Checkpoint'i yok etmek yerine pasif hale getirmek daha güvenli olabilir.
            hitCheckpoint.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Yanlýþ Checkpoint! Beklenen: " + (currentArrowCount + 1) + ", Basýlan: " + hitID);

            if (uiManager != null)
            {
                uiManager.ShowWrongArrowFeedback();
            }
        }
    }
}