using UnityEngine;
using UnityEditor; // Editor script'leri için bu kütüphane gereklidir.

[CustomEditor(typeof(RouteManager))]
public class RouteManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Önce varsayýlan Inspector elemanlarýný çizdiriyoruz.
        DrawDefaultInspector();

        // target, bu script'in þu an incelediði component'tir (yani RouteManager).
        RouteManager routeManager = (RouteManager)target;

        // Butonun daha belirgin olmasý için biraz boþluk býrakalým.
        EditorGUILayout.Space(10);

        // "Bake" butonunu oluþturuyoruz.
        if (GUILayout.Button("Bake Route Data to LevelData Asset", GUILayout.Height(40)))
        {
            // Butona týklandýðýnda bu özel metodu çaðýrýyoruz.
            BakeData(routeManager);
        }
    }

    private void BakeData(RouteManager manager)
    {
        // 1. Gerekli referanslarýn atanýp atanmadýðýný kontrol et.
        if (manager.LevelDataToBake == null)
        {
            Debug.LogError("Bake Failed: 'Level Data To Bake' alaný boþ. Lütfen bir LevelData asset'i atayýn.", manager);
            return;
        }

        if (manager.RoutePointIDs == null || manager.RoutePointIDs.Count < 2)
        {
            Debug.LogError("Bake Failed: 'Route Point IDs' listesinde en az 2 nokta olmalý.", manager);
            return;
        }

        // 2. Veriyi hesaplat.
        // RouteManager'a ekleyeceðimiz yeni metot ile verileri hesaplýyoruz.
        (float calculatedLength, int pointCount) = manager.PreviewAndCalculateRouteStats();

        // 3. LevelData asset'ini al ve veriyi içine yaz.
        LevelData dataAsset = manager.LevelDataToBake;
        dataAsset.BakeData(manager.RoutePointIDs, calculatedLength, pointCount);

        // 4. Deðiþiklikleri kaydet.
        // Bu satýr, Unity'e bu asset'in deðiþtiðini ve kaydedilmesi gerektiðini söyler.
        EditorUtility.SetDirty(dataAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=green>SUCCESS:</color> Route data from '{manager.gameObject.name}' was successfully baked into '{dataAsset.name}'.", dataAsset);
    }
}