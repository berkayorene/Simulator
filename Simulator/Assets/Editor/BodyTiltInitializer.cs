using UnityEditor;
using UnityEngine;

public class BodyTiltInitializer : EditorWindow
{
    private GameObject bus;
    private RCCP_BodyTilt rCCP_BodyTilt;
    [MenuItem("Tools/Bus Tilt")]
    public static void ShowWindow()
    {
        GetWindow<BodyTiltInitializer>("Bus Tilt");
    }
    private Vector2 scrollPos;

    private void OnGUI()
    {
        bus = (GameObject)EditorGUILayout.ObjectField("Source Object", bus, typeof(GameObject), true);

        rCCP_BodyTilt = bus.gameObject.GetComponentInChildren<RCCP_BodyTilt>();

        if (GUILayout.Button("Initialize tilt"))
        {
            rCCP_BodyTilt.InitializeTilt();
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        

        EditorGUILayout.EndScrollView();

        
    }
}
