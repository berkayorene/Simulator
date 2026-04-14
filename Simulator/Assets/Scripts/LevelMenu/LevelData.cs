using System.Collections.Generic;
using UnityEngine;

// Menü yolunu Ýngilizce yaptýk.
[CreateAssetMenu(fileName = "NewLevelData", menuName = "Route System/Level Data", order = 1)]
public class LevelData : ScriptableObject
{
    [Header("Level Information")]
    [Tooltip("The name of the level to be displayed in the menu.")]
    public string levelName;
    [Tooltip("The exact name of the scene to be loaded.")]
    public string sceneName;

    [Tooltip("The mission type description (e.g., Timed Lap, Passenger Transport).")]
    public string missionType;

    [Header("Route Definition (Baked from a RouteManager)")]
    [Tooltip("This data is a read-only snapshot from a RouteManager in a scene.")]
    [SerializeField] private List<string> routePointIDs;
    [SerializeField] private float totalRouteLength;
    [SerializeField] private int totalPointCount;

    public IReadOnlyList<string> RoutePointIDs => routePointIDs.AsReadOnly();
    public float TotalRouteLength => totalRouteLength;
    public int TotalPointCount => totalPointCount;

    /// Bu metot, Editör script'i tarafýndan çaðrýlarak bu asset'in verilerini günceller.
    public void BakeData(List<string> ids, float length, int count)
    {
        routePointIDs = new List<string>(ids);
        totalRouteLength = length;
        totalPointCount = count;
        Debug.Log($"Baking successful for {this.name}: {count} points, {length:F1}m length.");
    }
}