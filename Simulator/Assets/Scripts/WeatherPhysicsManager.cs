using UnityEngine;
using System.Collections.Generic; // You might not need this anymore

public class WeatherPhysicsManager : MonoBehaviour
{
    [Header("Scene References")]
    [Tooltip("The main ground collider for your road or terrain.")]
    public Collider planeCollider; // terrain
    public GameObject asphaltRoads;

    [Header("Physic Materials for Weather")]
    public PhysicsMaterial clearSkyMaterial;
    public PhysicsMaterial rainLightMaterial;
    public PhysicsMaterial rainMediumMaterial;
    public PhysicsMaterial rainStormMaterial;
    public PhysicsMaterial snowLightMaterial;
    public PhysicsMaterial snowHeavyMaterial;
    public PhysicsMaterial yagmurSisMaterial;    // Rain-Fog
    public PhysicsMaterial karSisMaterial;       // Snow-Fog
    public PhysicsMaterial yagmurKarMaterial;    // Sleet/Slush


    void Start()
    {
        // Ensure a default material is set on start
        if (planeCollider != null && clearSkyMaterial != null)
        {
            planeCollider.material = clearSkyMaterial;
            for (int i = 0; i < asphaltRoads.transform.childCount; i++)
            {
                Transform child = asphaltRoads.transform.GetChild(i);
                if (child.TryGetComponent<Collider>(out Collider childCollider))
                {
                    childCollider.material = clearSkyMaterial; // Apply the same material to each child collider
                }
            }
            Debug.Log("WeatherPhysicsManager initialized. Default weather is Clear Sky.");
        }
        else
        {
            Debug.LogError("The Plane Collider or the Clear Sky Material is not assigned in the Inspector!");
        }
    }

    /// <summary>
    /// Call this method from your UI buttons or game manager to change the weather.
    /// </summary>
    /// <param name="weatherName">The name of the weather preset.</param>
    public void ChangeGroundPhysicsGradually(string weatherName)  // not gradually yet, but can be extended. I left the name for the sake of easiness to attach to UI buttons.
    {
        if (planeCollider == null)
        {
            Debug.LogError("Cannot change physics, the Plane Collider is not assigned!");
            return;
        }

        PhysicsMaterial targetMaterial = null;

        switch (weatherName)
        {
            case "Clear Sky":
                targetMaterial = clearSkyMaterial;
                break;
            case "Rain - Spray":
                targetMaterial = rainLightMaterial;
                break;
            case "Rain - Medium":
                targetMaterial = rainMediumMaterial;
                break;
            case "Rain - Storm":
                targetMaterial = rainStormMaterial;
                break;
            case "Snow - Light":
                targetMaterial = snowLightMaterial;
                break;
            case "Snow - Heavy":
                targetMaterial = snowHeavyMaterial;
                break;
            case "Yagmur - Sis":
                targetMaterial = yagmurSisMaterial;
                break;
            case "Kar - Sis":
                targetMaterial = karSisMaterial;
                break;
            case "Yagmur - Kar":
                targetMaterial = yagmurKarMaterial;
                break;

            default:
                Debug.LogWarning("Weather preset '" + weatherName + "' not found!");
                return;
        }

        if (targetMaterial != null)
        {
            planeCollider.material = targetMaterial;
            for (int i = 0; i < asphaltRoads.transform.childCount; i++)
            {
                Transform child = asphaltRoads.transform.GetChild(i);
                if (child.TryGetComponent<Collider>(out Collider childCollider))
                {
                    childCollider.material = targetMaterial; // Apply the same material to each child collider
                }
            }
            Debug.Log("Ground physics changed to: " + weatherName);
        }
        else
        {
            Debug.LogWarning("The material for '" + weatherName + "' is not assigned in the Inspector!");
        }
    }
}