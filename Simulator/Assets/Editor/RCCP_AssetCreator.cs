using UnityEngine;
using UnityEditor;

public class RCCP_AssetCreator
{

    [MenuItem("Tools/RCCP/Create Ground Materials Asset")]
    public static void CreateGroundMaterialsAsset()
    {

        ScriptableObject asset = ScriptableObject.CreateInstance("RCCP_GroundMaterials");

        if (!asset)
        {
            Debug.LogError("Failed to create instance of RCCP_GroundMaterials. Make sure the script exists and has no compile errors.");
            return;
        }

        AssetDatabase.CreateAsset(asset, "Assets/MyGroundMaterials.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        Debug.Log("Successfully created MyGroundMaterials.asset in your main Assets folder!");

    }

}