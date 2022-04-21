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
            GetRandomPathInRange(5);

        if (!move)
            return;

        float step = _moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, path[pathStep].pointPosition, step);

        if (Vector3.Distance(transform.position, path[pathStep].pointPosition) < 0.1f)
        {
            if (pathStep + 1 < path.Count)
                pathStep++;
            else
            {
                move = false;
                OnDestinationReach();
            }
        }
    }

    //Denne funksjonen fungerer bra
    public void GetPath(Vector3 targetPosition)
    {
        _target = _levelGenerator.getGridSquareFromPosition(targetPosition);
        path = _levelGenerator._pathGenerator.GetPath(_levelGenerator.getGridSquareFromPosition(transform.position), _target);

        if (path.Count > 1)
            pathStep = 1;
        else pathStep = 0;

        move = true;
    }

    //Funksjonen fungerer, men ikke hver gang. Må fikses
    public Vector3 GetRandomPathInRange(float range)
    {
        Vector3 point = _levelGenerator._pathGenerator.getRandomAccesiblePosition(transform.position, range);

        GetPath(point);

        return point;
    }

    public void StopPath()
    {
        move = false;
    }

    public virtual void OnDestinationReach()
    {
        Debug.Log("Found last point");
    }

    public bool isMoving()
    {
        return move;
    }
}
