using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class Character_Movement : MonoBehaviour
{
    [Header("Movement")]
    public float movement_speed;

    [Header("Collision")]
    public float skinWidth;
    
    Collider2D _collider;

    bool _rightCheck;
    bool _leftCheck;
    bool _upCheck;
    bool _downCheck;

    //Maincam referance
    Camera mainCamera;

    private void Start()
    {
        _collider = GetComponent<Collider2D>();
        mainCamera = Camera.main;

        //Stop game if no camera is present
        if (mainCamera == null)
        {
            Debug.LogError("Error: Script Character_Movement requires a Camera Object with the MainCamera tag in scene. Game is ended");
            UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
        }
    }

    private void Update()
    {
        //WASD Movement
        float moveX = Input.GetAxisRaw("Horizontal") * movement_speed * Time.deltaTime;
        float moveY = Input.GetAxisRaw("Vertical") * movement_speed * Time.deltaTime;

        #region Kolisjonssjekk
        if (moveX > 0)
            _rightCheck = checkForCollision(_collider, Vector2.right, skinWidth);
        else if (moveX < 0)
            _leftCheck = checkForCollision(_collider, Vector2.left, skinWidth);

        if (moveY > 0)
            _upCheck = checkForCollision(_collider, Vector2.up, skinWidth);
        else if (moveY < 0)
            _downCheck = checkForCollision(_collider, Vector2.down, skinWidth);

        if (moveX > 0 && _rightCheck)
            moveX = 0;
        if (moveX < 0 && _leftCheck)
            moveX = 0;
        if (moveY > 0 && _upCheck)
            moveY = 0;
        if (moveY < 0 && _downCheck)
            moveY = 0;
        #endregion

        transform.position += new Vector3(moveX, moveY);
    }

    bool checkForCollision(Collider2D moveCollider, Vector2 direction, float distance)
    {
        if (moveCollider == null)
            return false;

        RaycastHit2D[] hits = new RaycastHit2D[10];
        ContactFilter2D filter = new ContactFilter2D();

        int count = moveCollider.Cast(direction, filter, hits, distance);

        for (int i = 0; i < count; i++)
        {
            if (!hits[i].collider.isTrigger)
                return true;
        }

        return false;
    }
}