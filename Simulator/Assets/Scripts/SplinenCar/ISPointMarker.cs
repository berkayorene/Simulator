using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class ISPointMarker : MonoBehaviour
{
    [Header("Kimlik")]
    [SerializeField] private string pointID;
    public string PointID => pointID;

    [Header("UI Referanslarý")]
    [SerializeField] private Button mainButton;
    [SerializeField] private TextMeshProUGUI stepText;
    [SerializeField] private Button selectButton;
    // YENÝ: Silme butonu için referans eklendi.
    [SerializeField] private Button deleteButton;

    private int currentRouteIndex = -1;

    public event Action<int> OnStepClicked;
    public event Action<string> OnSelectClicked;
    // YENÝ: Silme olayý eklendi.
    public event Action<string, int> OnDeleteClicked;

    private void Awake()
    {
        mainButton.onClick.AddListener(HandleMainClick);
        selectButton.onClick.AddListener(HandleSelectClick);
        // YENÝ: Silme butonu dinleyicisi eklendi.
        deleteButton.onClick.AddListener(HandleDeleteClick);
        ResetState();
    }

    public void ResetState()
    {
        stepText.gameObject.SetActive(false);
        selectButton.gameObject.SetActive(false);
        // YENÝ: Silme butonu da sýfýrlanmalý.
        deleteButton.gameObject.SetActive(false);
        mainButton.interactable = false;
        currentRouteIndex = -1;
    }

    public void SetAsRouteStep(int routeIndex)
    {
        this.currentRouteIndex = routeIndex;
        stepText.text = (routeIndex + 1).ToString();
        stepText.gameObject.SetActive(true);
        mainButton.interactable = true;
    }

    public void ShowSelectButton()
    {
        selectButton.gameObject.SetActive(true);
    }

    // YENÝ: Silme butonunu göstermek için metot eklendi.
    public void ShowDeleteButton()
    {
        deleteButton.gameObject.SetActive(true);
    }

    private void HandleMainClick()
    {
        if (currentRouteIndex != -1)
        {
            OnStepClicked?.Invoke(currentRouteIndex);
        }
    }

    private void HandleSelectClick()
    {
        OnSelectClicked?.Invoke(pointID);
    }

    // YENÝ: Silme butonu týklandýðýnda olayý tetikleyen metot.
    private void HandleDeleteClick()
    {
        // Silme iþlemi, noktanýn hangi index'te olduðunu bilmeyi gerektirir.
        if (currentRouteIndex != -1)
        {
            OnDeleteClicked?.Invoke(pointID, currentRouteIndex);
        }
    }
}
