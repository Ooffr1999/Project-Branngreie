using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    public float moveSpeed;
    public List<Points> path;

    public GameObject _player;
    public CharacterController _controller;
    public LevelGenerator _levelGenerator;

    private void Update()
    {
        path = _levelGenerator._pathGenerator.GetPath(_levelGenerator.getGridSquareFromPosition(transform.position), _levelGenerator.getGridSquareFromPosition(_player.transform.position));

        Vector3 point = path[0].pointPosition;
        Vector3 direction = Vector3.MoveTowards(transform.position, point, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(point, transform.position) < 0.5)
            path.Remove(path[0]);

        transform.position += direction;
    }
 
}
