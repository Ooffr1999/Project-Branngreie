using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Character_Movement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float zLimit;

    [Header("Jumping")]
    public float jumpHeight;
    public float gravityModifier;
    public bool isGrounded;

    [Space(5)]
    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask ground;

    [Header("Door interaction settings")]
    public float doorRange;
    public LayerMask doorLayer;

    public TransitionEffect effect;
    public UnityEvent onTransitionEvent;
    public UnityEvent onEndGameEvent;

    public CharacterController controller;
    public LevelGenerator _levelGenerator;

    Vector3 moveForce;

    private void Update()
    { 
        if (Input.GetKeyDown(KeyCode.K))
            _levelGenerator._pathGenerator.GetPath(GetPlayerPositionOnGrid(), _levelGenerator._roomEnd);

        Jump();

        GetMovement();

        if (Input.GetKeyDown(KeyCode.H) && Physics.CheckSphere(transform.position, doorRange, doorLayer))
        {
            GetEnd();
            effect.Play(1f, onTransitionEvent);
        }
    }

    public void GetEnd()
    {
        GameObject endDoor = GameObject.FindGameObjectWithTag("Finish");

        if (endDoor == null)
            return;

        //Enter endstate
        Debug.Log("Gratla, du vant spillet");
        onEndGameEvent.Invoke();
    }

    void GetMovement()
    {
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        controller.Move(transform.forward * moveZ + transform.right * moveX);

        if (transform.position.z <= zLimit)
            transform.position = new Vector3(transform.position.x, transform.position.y, zLimit);
    }

    void Jump()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, ground);

        if (isGrounded)
            moveForce.y = -2;
        else moveForce.y += (Physics.gravity.y * gravityModifier) * Time.deltaTime;
        
        if (Input.GetKey(KeyCode.Space) && isGrounded)
            moveForce.y = jumpHeight;

        controller.Move(moveForce * Time.deltaTime);
    }

    public Vector3 GetPlayerPositionOnGrid()
    {
        return _levelGenerator.getGridSquareFromPosition(transform.position);
    }
}
