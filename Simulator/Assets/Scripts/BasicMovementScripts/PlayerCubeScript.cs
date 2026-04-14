using Unity.VisualScripting;
using UnityEngine;

public class PlayerCubeScript : MonoBehaviour
{
    public float speed = 20000f;


    public float rotationSpeed = 20f;
    public float horizontalSpeed = 20f;


    void Update()
    {
        // float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right arrows
        float moveZ = Input.GetAxis("Vertical");   // W/S or Up/Down arrows


        float keyboardRotation = Input.GetAxis("Horizontal");   // A/D or LEFT/RIGHT arrows
        float mouseRotation = 2f * Input.GetAxis("Mouse X");
        float combinedRotation = (keyboardRotation + mouseRotation) ;

        Vector3 movement = new Vector3(0f, 0f, moveZ);

        // Vector3 rotationAroundY = new Vector3(0f, 60, 0f);



        transform.Translate(movement * speed * Time.deltaTime);
        transform.Rotate(0f, combinedRotation * Time.deltaTime * rotationSpeed, 0f);


        /* public float horizontalSpeed = 2.0F;
    public float verticalSpeed = 2.0F;
    void Update() {
        float h = horizontalSpeed * Input.GetAxis("Mouse X");
        float v = verticalSpeed * Input.GetAxis("Mouse Y");
        transform.Rotate(v, h, 0);
    }  */


    }
}
