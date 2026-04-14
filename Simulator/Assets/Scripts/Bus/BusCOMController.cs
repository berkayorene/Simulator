using UnityEngine;

public class BusCOMController : MonoBehaviour
{
    [SerializeField] private Rigidbody busRB;
    [SerializeField] private Transform COM;
    private Vector3 defaultCOMPosition = new Vector3(0, -0.6f, 0);

    private float passengerMass = 75;
    private float busMass;

    private float comShiftPerPassengerX = -0.05f;
    private float comShiftPerPassengerZ = -0.05f;
    private float comShiftPerPassengerY = -0.02f;
    

    void Start()
    {
        
    }

    void Update()
    {
    }

    public void UpdateCOM(Vector3 averagePassengerPosition, int totalPassengerNumber)
    {
        busRB.automaticCenterOfMass = false;
        COM.position = defaultCOMPosition;
        Vector3 newCOM = defaultCOMPosition;
        busMass = busRB.mass;

        newCOM = ((busMass * COM.position + (passengerMass*totalPassengerNumber) * averagePassengerPosition)) / (busMass + (passengerMass * totalPassengerNumber));

        /*
        newCOM.x += (positionOfPassengers.x - COM.position.x > 0) ? comShiftPerPassengerX * totalPassengerNumber : -comShiftPerPassengerX * totalPassengerNumber;
        newCOM.z += (positionOfPassengers.z - COM.position.z > 0) ? comShiftPerPassengerZ * totalPassengerNumber : -comShiftPerPassengerZ * totalPassengerNumber;
        newCOM.y += comShiftPerPassengerY; */

        COM.position = newCOM;
        busRB.centerOfMass = COM.position;

        Debug.Log($"New Center Of Mass: {newCOM}");
    }
}
