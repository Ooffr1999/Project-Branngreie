using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LevelGenerator))]
public class PathGenerator : MonoBehaviour
{
    public bool drawPoints = true;

    public Vector3 start;
    public Vector3 end;

    public LayerMask obstacleLayer;

    int _mapWidth, _mapDepth;
    float _sizeModifier;

    public List<Vector3> gridPoints = new List<Vector3>();

    public List<Vector3> openPoints = new List<Vector3>();
    public List<Vector3> closedPoints = new List<Vector3>();

    [HideInInspector]
    public static PathGenerator _instance;

    private void Awake()
    {
        if (_instance != null)
            Destroy(this);
        else _instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            Debug.Log(GetPath());
    }

    #region Find and Clear Path
    public void GenerateGrid(int mapWidth, int mapDepth, Vector3 startPosition, float sizeModifier)
    {
        gridPoints = MapGridGenerator.GenerateMapGrid(mapWidth, mapDepth, startPosition, sizeModifier);
        _mapWidth = mapWidth;
        _mapDepth = mapDepth;
        _sizeModifier = sizeModifier;
    }
    
    public bool GetPath()
    {
        int cost = 1;

        ClearPath();

        while (true)
        {
            List<Vector3> pointsToAdd = new List<Vector3>();

            for (int c = 0; c < openPoints.Count; c++)
            {
                Points[] directions = getDirections(openPoints[c], cost);

                for (int i = 0; i < 4; i++)
                {
                    if (evaluatePoint(directions[i]) && !pointsToAdd.Contains(directions[i].pointPosition))
                        pointsToAdd.Add(directions[i].pointPosition);
                }
            }

            closedPoints.AddRange(openPoints);
            openPoints.Clear();
            openPoints.AddRange(pointsToAdd);
            openPoints.Sort((p1, p2) => getPoint(p1, cost).F.CompareTo(getPoint(p2, cost).F));

            pointsToAdd.Clear();
            cost++;

            if (openPoints.Contains(end))
                return true;
            else if (openPoints.Count == 0)
                return false;
        }
    }
    
    public void ClearPath()
    {
        openPoints.Clear();
        closedPoints.Clear();
        openPoints.Add(start);
    }
    #endregion

    #region Get and Evaluate Directions
    Points[] getDirections(Vector3 currentPosition, int count)
    {
        Points[] directions = new Points[4];

        directions[0] = getPoint(new Vector3(currentPosition.x + _sizeModifier, currentPosition.y, currentPosition.z), count);
        directions[1] = getPoint(new Vector3(currentPosition.x, currentPosition.y, currentPosition.z - _sizeModifier), count);
        directions[2] = getPoint(new Vector3(currentPosition.x - _sizeModifier, currentPosition.y, currentPosition.z), count);
        directions[3] = getPoint(new Vector3(currentPosition.x, currentPosition.y, currentPosition.z + _sizeModifier), count);

        return directions;
    }

    bool evaluatePoint(Points point)
    {
        if (closedPoints.Contains(point.pointPosition))
            return false;

        else if (openPoints.Contains(point.pointPosition))
            return false;

        else if (!gridPoints.Contains(point.pointPosition))
            return false;

        else if (Physics.CheckBox(point.pointPosition, Vector3.one / 2, transform.rotation, obstacleLayer))
            return false;

        else return true;
    }
    #endregion

    #region Generate Important Positions
    public Vector3 getStartPos()
    {
        return start;
    }

    public Vector3 getEndPos()
    {
        return end;
    }

    public void GetStartAndEndPositions(Vector3 startDoorPosition, Vector3 endDoorPosition)
    {
        start = getPointClosestToPosition(startDoorPosition);
        end = getPointClosestToPosition(endDoorPosition);
    }

    Vector3 getPointClosestToPosition(Vector3 pointPosition)
    {
        Vector3 closestPoint = Vector3.zero;

        for (int i = 0; i < gridPoints.Count; i++)
        {
            if (Vector3.Distance(gridPoints[i], pointPosition) < Vector3.Distance(closestPoint, pointPosition))
                closestPoint = gridPoints[i];
        }

        return closestPoint;
    }
    #endregion

    #region GetValues
    Points getPoint(Vector3 pointPosition, int cost)
    {
        Points newPoint = new Points();

        newPoint.H = GetH(pointPosition);
        newPoint.G = cost;
        newPoint.F = GetF(newPoint);
        newPoint.pointPosition = pointPosition;

        return newPoint;
    }

    public List<Vector3> getGridPoints()
    {
        return gridPoints;
    }
    
    public float GetH(Vector3 pointPosition)
    {
        return Vector3.Distance(pointPosition, end);
    }

    public float GetF(Points point)
    {
        return point.H + point.G;
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying && !drawPoints)
            return;

        for (int i = 0; i < gridPoints.Count; i++)
        {
            Gizmos.color = Color.green;

            if (gridPoints[i] == start)
                Gizmos.color = Color.white;
            else if (gridPoints[i] == end)
                Gizmos.color = Color.black;

            Gizmos.DrawCube(gridPoints[i], Vector3.one / 2);
        }

        for (int i = 0; i < openPoints.Count; i++)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawCube(openPoints[i], Vector3.one / 2);
        }

        for (int i = 0; i < closedPoints.Count; i++)
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawCube(closedPoints[i], Vector3.one / 2);
        }
    }
}

[System.Serializable]
public class Points
{
    public float H;
    public float G;
    public float F;
    public Vector3 pointPosition;
    public Points parentPoint;
}