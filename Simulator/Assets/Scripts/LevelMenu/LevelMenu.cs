using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class LevelHoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public LevelData levelData;             // ScriptableObject’ten gelen veri
    public GameObject infoPanel;            // Panel (aç/kapa yapılacak)

    [Header("UI Elements in Info Panel")]
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI missionTypeText;
    public TextMeshProUGUI totalLengthText;
    public TextMeshProUGUI totalPointsText;

    public TextMeshProUGUI buttonText; // Buton üzerindeki metin

    private void Start()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (levelData != null && infoPanel != null)
        {
            // UI’ya veri bas
            if (levelNameText != null)
                levelNameText.text = levelData.levelName;

            if (missionTypeText != null)
                missionTypeText.text = levelData.missionType;

            if (totalLengthText != null)
                totalLengthText.text = $"Route Length: {levelData.TotalRouteLength:F1} m";

            if (totalPointsText != null)
                totalPointsText.text = $"Waypoints: {levelData.TotalPointCount}";

            if (buttonText != null)
                buttonText.enabled = false;

            infoPanel.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);

        if (buttonText != null)
            buttonText.enabled = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (levelData != null)
        {
            // Sahne yüklenmeden önce LevelGameManager’a bilgi ver
            if (LevelGameManager.Instance != null)
                LevelGameManager.Instance.currentLevelData = levelData;

            // Sahne yükle
            SceneManager.LoadScene(levelData.sceneName);
        }
    }
}
