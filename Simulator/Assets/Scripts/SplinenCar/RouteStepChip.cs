using UnityEngine;
using UnityEngine.UI;
using System;

public class RouteStepChip : MonoBehaviour
{
    [Tooltip("Bu çipin temsil ettiði rota adýmýnýn index'i (0'dan baþlar).")]
    public int routeIndex;

    [Tooltip("Index numarasýný gösterecek olan TextMeshPro elemaný.")]
    public TMPro.TextMeshProUGUI stepText;

    private Button chipButton;

    // Bu event, çipe týklandýðýnda RouteEditorPanel'e haber vermek için kullanýlýr.
    public event Action<int> OnChipClicked;

    private void Awake()
    {
        chipButton = GetComponent<Button>();
        if (chipButton != null)
        {
            chipButton.onClick.AddListener(HandleClick);
        }
    }

    /// <summary>
    /// Bu çipin bilgilerini ayarlar.
    /// </summary>
    public void Setup(int index)
    {
        this.routeIndex = index;
        if (stepText != null)
        {
            stepText.text = (index + 1).ToString(); // Index 0 ise ekranda "1" yazar.
        }
    }

    private void HandleClick()
    {
        // Týklandýðýnda, kendi index'ini dinleyenlere (RouteEditorPanel'e) bildir.
        OnChipClicked?.Invoke(routeIndex);
    }

    private void OnDestroy()
    {
        // Bellek sýzýntýlarýný önlemek için event'leri temizle.
        OnChipClicked = null;
        if (chipButton != null)
        {
            chipButton.onClick.RemoveListener(HandleClick);
        }
    }
}
