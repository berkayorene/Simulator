using Unity.VisualScripting;
using UnityEngine;

public class CarModelController : MonoBehaviour
{
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [SerializeField] private Transform frontLeftWheelMesh;
    [SerializeField] private Transform frontRightWheelMesh;
    [SerializeField] private Transform rearLeftWheelMesh;
    [SerializeField] private Transform rearRightWheelMesh;

    [SerializeField] private Transform steeringWheelMesh;
    private float steeringWheelMultiplillier = 16f;
    private float wheelMultiplillier = 0.5f;

    void Start()
    {
      
    }

    void Update()
    {
        SteerWheelAnimation();
        SteeringWheel();
    }

    void SteeringWheel()
    {
        float steerAngle = frontLeftWheelCollider.steerAngle;
        steeringWheelMesh.localRotation = Quaternion.Euler(0, steerAngle * steeringWheelMultiplillier, 0);
    }

    void SteerWheelAnimation()
    {
        Vector3 pos;
        Quaternion rot;

        frontLeftWheelCollider.GetWorldPose(out pos, out rot);
        frontLeftWheelMesh.rotation = rot;


        frontRightWheelCollider.GetWorldPose(out pos, out rot);
        frontRightWheelMesh.rotation = rot;

        rearLeftWheelCollider.GetWorldPose(out pos, out rot);
        rearLeftWheelMesh.rotation = rot;

        rearRightWheelCollider.GetWorldPose(out pos, out rot);
        rearRightWheelMesh.rotation = rot;
    }
}
