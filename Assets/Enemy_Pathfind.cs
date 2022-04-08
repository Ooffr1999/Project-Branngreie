using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Pathfind : MonoBehaviour
{
    public float _move;
    public GameObject _target;

    public int pathStep = 0;
    public List<Points> path = new List<Points>();
    public LevelGenerator _levelGenerator;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            GetPath();

        float step = _move * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, path[pathStep].pointPosition, step);

        if (Vector3.Distance(transform.position, path[pathStep].pointPosition) < 0.25)
        {
            pathStep++;
        }
    }

    void GetPath()
    {
        path = _levelGenerator._pathGenerator.GetPath(_levelGenerator.getGridSquareFromPosition(transform.position), _levelGenerator.getGridSquareFromPosition(_target.transform.position));
        pathStep = 0;
    }
}
