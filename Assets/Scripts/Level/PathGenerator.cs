using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LevelGenerator))]
public class PathGenerator : MonoBehaviour
{
    public Vector3 start, end;

    int _mapWidth, _mapDepth;
    float _sizeModifier;

    List<Vector3> gridPoints = new List<Vector3>();

    public List<Points> openPoints = new List<Points>();
    public List<Points> closedPoints = new List<Points>();

    public List<Points> pathPoints = new List<Points>();

    [HideInInspector]
    public static PathGenerator _instance;

    private void Awake()
    {
        if (_instance != null)
            Destroy(this);
        else _instance = this;
    }

    #region Find and Clear Path
    public void GenerateGrid(int mapWidth, int mapDepth, Vector3 startPosition, float sizeModifier)
    {
        gridPoints = MapGridGenerator.GenerateMapGrid(mapWidth, mapDepth, startPosition, sizeModifier);
        _mapWidth = mapWidth;
        _mapDepth = mapDepth;
        _sizeModifier = sizeModifier;
    }
    
    public bool CheckPath(Vector3 pointStart, Vector3 pointEnd, LayerMask obstacleLayer)
    {
        ClearPath();

        openPoints.Add(getPoint(pointStart, 0));

        int cost = 1;

        while (true)
        {
            List<Points> pointsToAdd = new List<Points>();

            for (int c = 0; c < openPoints.Count; c++)
            {
                if (openPoints[c].pointPosition == pointEnd)
                {
                    //GetPath(pointStart);
                    return true;
                    
                }

                Points[] directions = getDirections(openPoints[c].pointPosition, openPoints[c].pointPosition, cost);

                for (int i = 0; i < 4; i++)
                {
                    if (evaluatePoint(directions[i], obstacleLayer) && !pointsToAdd.Contains(directions[i]))
                        pointsToAdd.Add(directions[i]);
                }
            }

            pointsToAdd = pointsToAdd.Distinct().ToList();
            closedPoints.AddRange(openPoints);
            closedPoints = closedPoints.Distinct().ToList();
            openPoints.Clear();
            openPoints.AddRange(pointsToAdd);
            openPoints.Sort((p1, p2) => p1.F.CompareTo(p2.F));
            openPoints = openPoints.Distinct().ToList();

            pointsToAdd.Clear();
            cost++;

            if (openPoints.Count == 0)
            {
                return false;
            }
        }
    }
    
    void GetPath(Vector3 pointStart)
    {
        pathPoints.Clear();

        pathPoints.Add(openPoints[0]);
        pathPoints.Add(new Points());

        for (int i = 1; i < 100; i++)
        {
            pathPoints[i] = closedPoints.Find(p => p.pointPosition == pathPoints[i - 1].parentPosition);

            if (pathPoints[i].pointPosition == pointStart)
                break;
            else pathPoints.Add(new Points());
        }
    }

    public void ClearPath()
    {
        openPoints.Clear();
        closedPoints.Clear();
    }
    #endregion

    #region Get and Evaluate Directions
    Points[] getDirections(Vector3 currentPosition, Vector3 parentPosition, int count)
    {
        Points[] directions = new Points[4];

        directions[0] = getPoint(new Vector3(currentPosition.x + _sizeModifier, currentPosition.y, currentPosition.z), parentPosition, count);
        directions[1] = getPoint(new Vector3(currentPosition.x, currentPosition.y, currentPosition.z - _sizeModifier), parentPosition, count);
        directions[2] = getPoint(new Vector3(currentPosition.x - _sizeModifier, currentPosition.y, currentPosition.z), parentPosition, count);
        directions[3] = getPoint(new Vector3(currentPosition.x, currentPosition.y, currentPosition.z + _sizeModifier), parentPosition, count);

        return directions;
    }

    public bool evaluatePoint(Points point, LayerMask obstacleLayer)
    {
        Points evaluatePoint = new Points();

        evaluatePoint = closedPoints.Find(p => p.pointPosition == point.pointPosition);
        
        if (evaluatePoint != null)
            return false;

        //if (closedPoints.Contains(point))
        //return false;

        evaluatePoint = openPoints.Find(p => p.pointPosition == point.pointPosition);
        
        if (evaluatePoint != null)
            return false;

        //else if (openPoints.Contains(point))
        //return false;

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

    public Vector3 getPointClosestToPosition(Vector3 pointPosition)
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

    #region Get Values

    public Points getPoint(Vector3 pointPosition, int cost)
    {
        Points newPoint = new Points();

        newPoint.H = GetH(pointPosition);
        newPoint.G = cost;
        newPoint.F = GetF(newPoint);
        newPoint.pointPosition = pointPosition;

        return newPoint;
    }

    public Points getPoint(Vector3 pointPosition, Vector3 parentPosition, int cost)
    {
        Points newPoint = new Points();

        newPoint.H = GetH(pointPosition);
        newPoint.G = cost;
        newPoint.F = GetF(newPoint);
        newPoint.pointPosition = pointPosition;
        newPoint.parentPosition = parentPosition;

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

    private void OnDrawGizmosSelected()
    {
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

            Gizmos.DrawCube(openPoints[i].pointPosition, Vector3.one / 2);
        }

        for (int i = 0; i < closedPoints.Count; i++)
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawCube(closedPoints[i].pointPosition, Vector3.one / 2);
        }

        for (int i = 0; i < pathPoints.Count; i++)
        {
            Gizmos.color = Color.cyan;

            Gizmos.DrawCube(pathPoints[i].pointPosition, Vector3.one / 2);
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
    public Vector3 parentPosition;
}