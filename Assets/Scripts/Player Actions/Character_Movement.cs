using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Character_Movement : MonoBehaviour
{
    public float moveSpeed;
    public float zLimit;

    public float doorRange;
    public LayerMask doorLayer;

    public TransitionEffect effect;
    public UnityEvent onTransitionEvent;
    public UnityEvent onEndGameEvent;

    public CharacterController controller;

    private void Update()
    {
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
}
