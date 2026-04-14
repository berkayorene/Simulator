using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class IDValidator : EditorWindow
{
    private Dictionary<string, List<GameObject>> splineDuplicates = new Dictionary<string, List<GameObject>>();
    private Dictionary<string, List<GameObject>> pointDuplicates = new Dictionary<string, List<GameObject>>();
    private Vector2 scrollPosition;

    // Adds a menu item to open the validator window
    [MenuItem("Araçlar/ID Validator")]
    public static void ShowWindow()
    {
        GetWindow<IDValidator>("ID Validator");
    }

    private void OnGUI()
    {
        GUILayout.Label("ID Uniqueness Validator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Find Duplicate IDs"))
        {
            FindDuplicates();
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Display Spline Duplicates
        if (splineDuplicates.Count > 0)
        {
            DisplayDuplicates("Spline", splineDuplicates);
        }
        else
        {
            EditorGUILayout.HelpBox("No duplicate SplineIDs found.", MessageType.Info);
        }

        GUILayout.Space(10);

        // Display Point Duplicates
        if (pointDuplicates.Count > 0)
        {
            DisplayDuplicates("Intersection", pointDuplicates);
        }
        else
        {
            EditorGUILayout.HelpBox("No duplicate IntersectionIDs found.", MessageType.Info);
        }

        EditorGUILayout.EndScrollView();
    }

    private void FindDuplicates()
    {
        // Find all ISSpline and ISPoint objects in the scene
        var allSplines = Object.FindObjectsByType<ISSpline>(FindObjectsSortMode.None);
        var allPoints = Object.FindObjectsByType<ISPoint>(FindObjectsSortMode.None);

        // Group by ID and filter for groups with more than one object
        splineDuplicates = allSplines
            .Where(s => !string.IsNullOrEmpty(s.SplineID))
            .GroupBy(s => s.SplineID)
            .Where(g => g.Count() > 1)
            .ToDictionary(g => g.Key, g => g.Select(s => s.gameObject).ToList());

        pointDuplicates = allPoints
            .Where(p => !string.IsNullOrEmpty(p.IntersectionID))
            .GroupBy(p => p.IntersectionID)
            .Where(g => g.Count() > 1)
            .ToDictionary(g => g.Key, g => g.Select(p => p.gameObject).ToList());

        // Force a repaint to show the results
        Repaint();
    }

    private void DisplayDuplicates(string type, Dictionary<string, List<GameObject>> duplicates)
    {
        EditorGUILayout.LabelField($"Duplicate {type} IDs:", EditorStyles.boldLabel);
        foreach (var entry in duplicates)
        {
            EditorGUILayout.HelpBox($"ID '{entry.Key}' is used by {entry.Value.Count} objects.", MessageType.Error);
            foreach (var obj in entry.Value)
            {
                // Create a button that, when clicked, pings the object in the Hierarchy
                if (GUILayout.Button($"-> {obj.name}"))
                {
                    EditorGUIUtility.PingObject(obj);
                }
            }
        }
    }
}