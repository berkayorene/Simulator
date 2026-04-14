using UnityEngine;

public class LookAround : MonoBehaviour
{
    public float mouseSensitivity = 100f;

    private float xRotation = 0f;

    void Start()
    {

    }

    void Update()
    {
        Look();
        
    }

    void Look()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.parent.Rotate(Vector3.up * mouseX);

        }
        else
        {

            transform.parent.localRotation = Quaternion.Slerp(
                transform.parent.localRotation,
                transform.parent.parent.localRotation,
                Time.deltaTime * 5f 
            );

        }
    }

}
