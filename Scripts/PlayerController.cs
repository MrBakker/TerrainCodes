using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // This script moves the character controller forward
    // and sideways based on the arrow keys.
    // It also jumps when pressing space.
    // Make sure to attach a character controller to the same game object.
    // It is recommended that you make only one call to Move or SimpleMove per frame.

    CharacterController characterController;

    public GameObject playerHead;

    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    private Vector3 moveDirection = Vector3.zero;

    void Start()
    {
        characterController = playerHead.transform.parent.GetComponent<CharacterController>();
        
    }

    void Update()
    {
        if (characterController.isGrounded)
        {
            // We are grounded, so recalculate
            // move direction directly from axes

            moveDirection = Vector3.zero;

            float yRotation = -playerHead.transform.eulerAngles.y;

            if (Input.GetAxis("Horizontal") < 0)
            {
                yRotation += 180f;
                
                //Debug.Log("MoveLeft");
                //Debug.Log("Yrotation: " + yRotation);
            }
            else
            {
                yRotation += 0f;
                //Debug.Log("MoveRight");
                //Debug.Log("Yrotation: " + yRotation);
            }

            moveDirection.x += Mathf.Cos(yRotation / 180 * Mathf.PI) * speed * Mathf.Abs(Input.GetAxis("Horizontal"));
            moveDirection.z += Mathf.Sin(yRotation / 180 * Mathf.PI) * speed * Mathf.Abs(Input.GetAxis("Horizontal"));

            // Debug.Log(moveDirection.x);
            // Debug.Log(moveDirection.z);

            yRotation = -playerHead.transform.eulerAngles.y;

            if (Input.GetAxis("Vertical") < 0)
            {
                yRotation -= 90f;
                //Debug.Log("MoveBackwards");
            }
            else
            {
                yRotation += 90f;
                //Debug.Log("MoveForwards");
            }

            moveDirection.x += Mathf.Cos(yRotation / 180 * Mathf.PI) * speed * Mathf.Abs(Input.GetAxis("Vertical"));
            moveDirection.z += Mathf.Sin(yRotation / 180 * Mathf.PI) * speed * Mathf.Abs(Input.GetAxis("Vertical"));

            if (Input.GetButton("Jump"))
            {
                moveDirection.y = jumpSpeed;
            }
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        moveDirection.y -= gravity * Time.deltaTime;

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);
    }
}