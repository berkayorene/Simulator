using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;

    private float speed = 12f;
    private float gravity = -9.81f * 2;
    private float jumpHeight = 3;

    [SerializeField] private Transform groundCheck;
    private float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;

    bool isGrounded;
    bool isMoving;

    private Vector3 lastPosition = new Vector3(0f, 0f, 0f);

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }


    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // move vector
        Vector3 move = transform.right * x + transform.forward * z;

        // move the player
        characterController.Move(move * speed * Time.deltaTime);

        // jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Going up
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Falling down
        velocity.y += gravity * Time.deltaTime;

        // Executing the jump
        characterController.Move(velocity * Time.deltaTime);

        isMoving = (lastPosition != gameObject.transform.position && isGrounded) ? true : false;

        lastPosition = gameObject.transform.position;
    }
}
