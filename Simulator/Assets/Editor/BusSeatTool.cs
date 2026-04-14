using UnityEditor;
using UnityEngine;

public class BusSeatToolWindow : EditorWindow
{
    private BusSeatController busSeatController;
    private BusCOMController busCOMController;

    [MenuItem("Tools/Bus Seat Tool")]
    public static void ShowWindow()
    {
        GetWindow<BusSeatToolWindow>("Bus Seat Tool");
    }

    private Vector2 scrollPos;

    private void OnGUI()
    {
        
        busSeatController = (BusSeatController)EditorGUILayout.ObjectField("Bus Seat Data", busSeatController, typeof(BusSeatController), true);
        busCOMController = (BusCOMController)EditorGUILayout.ObjectField("Bus Seat COM Controller", busCOMController, typeof(BusCOMController), true);

        if (busSeatController == null)
            return;

        busSeatController.rows = EditorGUILayout.IntField("Rows", busSeatController.rows);
        busSeatController.columns = EditorGUILayout.IntField("Columns", busSeatController.columns);

        if (GUILayout.Button("Generate Seats"))
        {
            busSeatController.OnValidate();
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        int index = 0;
        for (int row = 0; row < busSeatController.rows; row++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int col = 0; col < busSeatController.columns; col++)
            {
                if (GUILayout.Button(busSeatController.seats[index].GetIsOccupied() ? "X" : "O", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    bool occupied = !busSeatController.seats[index].GetIsOccupied();
                    busSeatController.seats[index].SetIsOccupied(occupied);
                }
                index++;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Calculate COM"))
        {
            CalculateCOM();
        }
    }

    private void CalculateCOM()
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (var seat in busSeatController.seats)
        {
            if (seat.GetIsOccupied())
            {
                sum += seat.GetLocalPosition(); 
                count++;
            }
        }

        
        Vector3 com = sum / count;
        
        //busSeatController.GetComponent<Rigidbody>().centerOfMass = com;
        busCOMController.UpdateCOM(com, count);

        
    }
}
