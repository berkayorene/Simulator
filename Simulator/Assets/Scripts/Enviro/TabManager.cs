using UnityEngine;

public class TabManager : MonoBehaviour
{
    // --- Değişkenlerinde bir değişiklik yok ---
    public GameObject havaKontrolPaneli;
    public GameObject aiTrafficPaneli;
    public GameObject VehicleFaultsScrollView;

    public SlidePanel havaKontrolSlider;
    public SlidePanel aiTrafficSlider;
    public SlidePanel vehicleFaultsSlider;

    public GameObject[] buttonsToHideWhenHavaKontrolOpen;
    public GameObject[] buttonsToHideWhenAITrafficOpen;

    // NOT: Bu gereksiz değişkeni kaldırdık.
    // private bool VehicleFaultScrollViewActif = false;


    public void ToggleHavaKontrol()
    {
        if (havaKontrolSlider != null)
            havaKontrolSlider.TogglePanel();

        bool panelActifMi = havaKontrolSlider.IsVisible();

        foreach (GameObject button in buttonsToHideWhenHavaKontrolOpen)
            if (button != null)
                button.SetActive(!panelActifMi);
    }

    public void ToggleAITraffic()
    {
        if (aiTrafficSlider != null)
            aiTrafficSlider.TogglePanel();

        bool panelActifMi = aiTrafficSlider.IsVisible();

        foreach (GameObject button in buttonsToHideWhenAITrafficOpen)
            if (button != null)
                button.SetActive(!panelActifMi);
    }

    // --- BU FONKSİYON DÜZELTİLDİ ---
    public void ToggleVehicleFaultsScrollView()
    {
        // 1. Paneli aç/kapatması için slider'ı tetikle.
        if (vehicleFaultsSlider != null)
            vehicleFaultsSlider.TogglePanel();

        // 2. Panelin güncel görünürlük durumunu kendisinden öğren.
        bool panelActifMi = vehicleFaultsSlider.IsVisible();

        // 3. Butonları bu duruma göre gizle veya göster.
        // (Hava Kontrol panelinin gizlediği butonları kullandığı varsayıldı)
        foreach (GameObject button in buttonsToHideWhenHavaKontrolOpen)
            if (button != null)
                button.SetActive(!panelActifMi);
    }

    public void CloseHavaKontrol()
    {
        if (havaKontrolSlider != null)
            havaKontrolSlider.ResetToOriginal();

        foreach (GameObject button in buttonsToHideWhenHavaKontrolOpen)
            if (button != null)
                button.SetActive(true);
    }

    public void CloseAITraffic()
    {
        if (aiTrafficSlider != null)
            aiTrafficSlider.ResetToOriginal();

        foreach (GameObject button in buttonsToHideWhenAITrafficOpen)
            if (button != null)
                button.SetActive(true);
    }

    // --- BU FONKSİYON DÜZELTİLDİ ---
    public void CloseVehicleFaultsScrollView()
    {
        // Paneli kapatmak için slider'ı orijinal pozisyonuna geri çek.
        if (vehicleFaultsSlider != null)
            vehicleFaultsSlider.ResetToOriginal();

        // İlgili butonları tekrar görünür yap.
        foreach (GameObject button in buttonsToHideWhenHavaKontrolOpen)
            if (button != null)
                button.SetActive(true);
    }
}