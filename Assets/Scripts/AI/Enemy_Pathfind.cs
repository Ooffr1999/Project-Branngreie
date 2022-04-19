using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Pathfind : MonoBehaviour
{
    public float _moveSpeed;
    public Vector3 _target;

    bool move;

    int pathStep = 0;
    List<Points> path = new List<Points>();
    LevelGenerator _levelGenerator;

    private void Start()
    {
        _levelGenerator = GameObject.FindGameObjectWithTag("LevelGenerator").GetComponent<LevelGenerator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            GetRandomPath();

        if (!move)
            return;

        float step = _moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, path[pathStep].pointPosition, step);

        if (Vector3.Distance(transform.position, path[pathStep].pointPosition) < 0.25)
        {
            pathStep++;
        }

        if (Vector3.Distance(transform.position, _target) < 0.25)
            move = false;
    }

    //Denne funksjonen fungerer bra
    public void GetPath(Vector3 targetPosition)
    {
        _target = targetPosition;
        path = _levelGenerator._pathGenerator.GetPath(_levelGenerator.getGridSquareFromPosition(transform.position), _levelGenerator.getGridSquareFromPosition(_target));
        pathStep = 0;
        move = true;
    }

    //Funksjonen fungerer, men ikke hver gang. Må fikses
    public void GetRandomPath()
    {
        for (int i = 0; i < 10; i++)
        {
            int randomGridPoint = Random.Range(0, _levelGenerator.grid.Count);

            if (_levelGenerator._pathGenerator.evaluatePoint(_levelGenerator._pathGenerator.getPoint(_levelGenerator.grid[randomGridPoint], Vector3.zero, Vector3.zero, 0)))
            {

                if (_levelGenerator._pathGenerator.CheckPath(_levelGenerator.getGridSquareFromPosition(transform.position), _levelGenerator.getGridSquareFromPosition(_levelGenerator.grid[randomGridPoint])))
                {
                    GetPath(_levelGenerator.grid[randomGridPoint]);
                    return;
                }
            }
        }
    }
}
