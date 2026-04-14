using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    private float mouseSensitivity = 500.0f;

    float xRotation = 0f;
    float yRotation = 0f;

    float topClamp = -90f;
    float bottomClamp = 90f;

    // Start is called before the first frame update
    void Start()
    {
        //Fps game i dont need cursor unless i make inventory system or something like that
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Look up and down
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, topClamp, bottomClamp);

        // Look left and right
        yRotation += mouseX;

        //Apply rotations
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);


    }
}
