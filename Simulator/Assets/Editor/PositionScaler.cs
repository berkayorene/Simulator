using UnityEngine;
using UnityEditor;

public class PositionScaler
{
    // Büyütme katsayýsýný burada sabit olarak belirliyoruz.
    private const float SCALE_FACTOR = 3.0f;

    // Bu metot, Unity'nin üst menüsüne yeni bir seçenek ekler.
    [MenuItem("Tools/Seçili Objenin Çocuk Pozisyonlarýný 3 Kat Büyüt")]
    private static void ScaleChildPositions()
    {
        // O an Hierarchy'de seçili olan objeyi al.
        Transform root = Selection.activeTransform;

        // Eðer hiçbir obje seçili deðilse uyarý ver ve iþlemi durdur.
        if (root == null)
        {
            EditorUtility.DisplayDialog("Hata", "Lütfen Hierarchy'den tüm sahne objelerini içeren kök objeyi (MAP_ROOT) seçin.", "Tamam");
            return;
        }

        // Kullanýcýya son bir kez sor. Bu iþlem geri alýnamaz.
        if (EditorUtility.DisplayDialog("Onay",
            "'" + root.name + "' objesinin altýndaki tüm nesnelerin pozisyonu " + SCALE_FACTOR + " ile çarpýlacak. Bu iþlem geri alýnamaz. Emin misiniz?",
            "Evet, Pozisyonlarý Büyüt", "Ýptal"))
        {
            // Kök objenin altýndaki tüm çocuklarý (pasif olanlar dahil) bul.
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                // Kök objenin kendisini atla, onun pozisyonu zaten (0,0,0) olmalý.
                if (child == root) continue;

                // Ýþte sihirli satýr: Çocuðun mevcut pozisyonunu 3 ile çarp.
                child.position *= SCALE_FACTOR;
            }

            Debug.Log("Pozisyonlar baþarýyla 3 kat büyütüldü!");
        }
    }
}